using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windore.EvolutionSimulation.Objects;
using Windore.Simulations2D;
using Windore.Simulations2D.Data;
using Windore.Simulations2D.Util.SMath;

namespace Windore.EvolutionSimulation
{
    public class Manager : SimulationManager
    {
        private ChangingVariable BaseEnvToxicity { get; set; } = new ChangingVariable(0, 0, 100, 0);
        private ChangingVariable BaseEnvTemperature { get; set; } = new ChangingVariable(30, 0, 100, 0);
        private ChangingVariable BaseEnvGroundNutrientContent { get; set; } = new ChangingVariable(10, 0, 20, 0);

        public Objects.Environment Env1 { get; private set; }
        public Objects.Environment Env2 { get; private set; }
        public Objects.Environment Env3 { get; private set; }

        public DataWindowManager DataWindowManager { get; } = new DataWindowManager();

        public Objects.Environment[] Envs => new Objects.Environment[]{ Env1, Env2, Env3 };

        [DataPoint("Duration")]
        public ulong Duration { get => SimulationScene.Age; }

        [DataPoint("PlantAmount")]
        public int PlantAmount { get => SimulationScene.SimulationObjects.Where(obj => obj is Plant).Count(); }
        [DataPoint("AnimalAmount")]
        public int AnimalAmount { get => SimulationScene.SimulationObjects.Where(obj => obj is Animal).Count(); }

        public Dictionary<string, DataCollector.Data> PlantsData { get; private set; } = new Dictionary<string, DataCollector.Data>();
        public Dictionary<string, DataCollector.Data> AnimalsData { get; private set; } = new Dictionary<string, DataCollector.Data>();


        private DataCollector collector = new DataCollector();
        private Dictionary<string, FileLogger> loggers = new Dictionary<string, FileLogger>();

        public StackPanel SelectedObjectPanel
        {
            get
            {
                StackPanel panel = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Vertical
                };

                if (SimulationScene.SelectedSimulationObject == null || SimulationScene.SelectedSimulationObject.IsRemoved)
                {
                    panel.Children.Add(NewSelectedPanelText("No Selection"));
                    return panel;
                }

                switch (SimulationScene.SelectedSimulationObject)
                {
                    case Plant plnt:
                        panel.Children.Add(new TextBlock { Text = "Plant", Margin = new Avalonia.Thickness(5, 5, 5, 0), FontSize = 16, FontWeight = Avalonia.Media.FontWeight.Bold });
                        AddOrganismToPanel(panel, plnt);

                        foreach (KeyValuePair<string, Property> pair in plnt.Properties.Properties)
                        {
                            panel.Children.Add(NewSelectedPanelText($"{pair.Key}: {Math.Round(pair.Value.Value, 3)}"));
                        }
                        break;
                    case Animal animal:
                        panel.Children.Add(new TextBlock { Text = "Animal", Margin = new Avalonia.Thickness(5, 5, 5, 0), FontSize = 16, FontWeight = Avalonia.Media.FontWeight.Bold });
                        AddOrganismToPanel(panel, animal);
                        panel.Children.Add(NewSelectedPanelText($"Stored Food: {Math.Round(animal.StoredFood, 3)}"));

                        foreach (KeyValuePair<string, Property> pair in animal.Properties.Properties)
                        {
                            panel.Children.Add(NewSelectedPanelText($"{pair.Key}: {Math.Round(pair.Value.Value, 3)}"));
                        }
                        break;
                    case Objects.Environment env:
                        AddEnvironmentToPanel(panel, env);
                        break;
                }

                return panel;
            }
        }

        private void AddOrganismToPanel(StackPanel panel, Organism organism)
        {
            panel.Children.Add(NewSelectedPanelText($"Current Energy: {Math.Round(organism.CurrentEnergy, 3)}"));
            panel.Children.Add(NewSelectedPanelText($"Energy Production: {Math.Round(organism.EnergyProduction, 3)}"));
            panel.Children.Add(NewSelectedPanelText($"Energy Consumption: {Math.Round(organism.EnergyConsumption, 3)}"));
            panel.Children.Add(NewSelectedPanelText($"Energy Storing Capacity: {Math.Round(organism.EnergyStoringCapacity, 3)}"));
            panel.Children.Add(NewSelectedPanelText($"Current Size: {Math.Round(organism.CurrentSize, 3)}"));
            panel.Children.Add(NewSelectedPanelText($"Current Age: {organism.Age}"));
        }

        private void AddEnvironmentToPanel(StackPanel panel, Objects.Environment env)
        {
            panel.Children.Add(new TextBlock { Text = "Environment", Margin = new Avalonia.Thickness(5, 5, 5, 0), FontSize = 16, FontWeight = Avalonia.Media.FontWeight.Bold });
            panel.Children.Add(NewSelectedPanelText($"Toxicity: {Math.Round(env.Toxicity.Value, 3)}"));
            panel.Children.Add(NewSelectedPanelText($"Temperature: {Math.Round(env.Temperature.Value, 3)}"));
            panel.Children.Add(NewSelectedPanelText($"Ground Nutrient Content: {Math.Round(env.GroundNutrientContent.Value, 3)}"));
            panel.Children.Add(NewSelectedPanelText($"Plant Amount in Env: {env.PlantAmount}"));
            panel.Children.Add(NewSelectedPanelText($"Animal Amount in Env: {env.AnimalAmount}"));

            Button plantPanelBtn = new Button
            {
                Margin = new Avalonia.Thickness(5),
                Height = 30,
                Content = "Open Env Plants Panel"
            };

            Button animalPanelBtn = new Button
            {
                Margin = new Avalonia.Thickness(5),
                Height = 30,
                Content = "Open Env Animals Panel"
            };

            plantPanelBtn.Click += (_, __) =>
            {
                DataWindowManager.OpenWindow(env.PlantsData, $"{env.Name} Environment Plants");
            };

            animalPanelBtn.Click += (_, __) =>
            {
                DataWindowManager.OpenWindow(env.AnimalsData, $"{env.Name} Environment Animals");
            };

            panel.Children.Add(plantPanelBtn);
            panel.Children.Add(animalPanelBtn);
        }

        private TextBlock NewSelectedPanelText(string text) 
        {
            return new TextBlock()
            {
                Text = text,
                Margin = new Avalonia.Thickness(5, 5, 5, 0),
                FontSize = 14,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
        }

        public Manager(SimulationScene scene) : base(scene)
        {
            
        }

        protected override void BeforeUpdate()
        {
            BaseEnvToxicity.Update();
            BaseEnvTemperature.Update();
            BaseEnvGroundNutrientContent.Update();

            if (Duration == 30_000)
            {
                SimulationSettings.Instance.AddStartingAnimal();
            }
        }

        protected override void AfterUpdate()
        {
            if (SimulationScene.Age % 200 == 0) 
            {
                Log();
            }
        }

        public void SetUpEnvs() 
        {
            double envSize = (SimulationScene.Width + SimulationScene.Height) / 2d * 0.415d;

            Env1 = new Objects.Environment(new Point(SimulationScene.Width * 0.25, SimulationScene.Height * 0.33), envSize)
            {
                Toxicity = new ChangingVariable(0, 0, 0, 0, BaseEnvToxicity),
                Temperature = new ChangingVariable(0, 0, 0, 0, BaseEnvTemperature),
                GroundNutrientContent = new ChangingVariable(0, 0, 0, 0, BaseEnvGroundNutrientContent),
                Name = "Normal"
            };

            Env2 = new Objects.Environment(new Point(SimulationScene.Width * 0.75, SimulationScene.Height * 0.33), envSize)
            {
                Toxicity = new ChangingVariable(0, 0, 0, 0, BaseEnvToxicity),
                Temperature = new ChangingVariable(10, 0, 10, 0, BaseEnvTemperature),
                GroundNutrientContent = new ChangingVariable(0, 0, 0, 0, BaseEnvGroundNutrientContent),
                Name = "Temperature"
                
            };

            Env3 = new Objects.Environment(new Point(SimulationScene.Width * 0.50, SimulationScene.Height * 0.66), envSize)
            {
                Toxicity = new ChangingVariable(0, 0, 0, 0, BaseEnvToxicity),
                Temperature = new ChangingVariable(0, 0, 0, 0, BaseEnvTemperature),
                GroundNutrientContent = new ChangingVariable(0, 0, 0, 0, BaseEnvGroundNutrientContent),
                Name = "Toxicity"
            };

            SimulationScene.AddAll(Env1, Env2, Env3);
        }

        public Objects.Environment GetEnvironment(Point position) 
        {
            Objects.Environment[] envs = { Env1, Env2, Env3 };
            foreach (Objects.Environment env in envs)
            {
                if (position.DistanceToSqr(env.Position) <= env.Shape.Height * env.Shape.Width / 4) 
                {
                    return env;
                }
            }
            return null;
        }

        private void Log() 
        {
            if (loggers.Count == 0) 
            {
                throw new Exception("Cannot log data because logging has not been set up yet.");
            }

            LogAllAnimals();
            LogAllPlants();

            int cnt = 1;
            foreach(Objects.Environment env in Envs) 
            {
                // Number cnt is used to log the same environment with the same logger
                LogEnvPlants(env, cnt);
                LogEnvAnimals(env, cnt);
                cnt++;
            }
        }

        private void LogEnvPlants(Objects.Environment env, int cnt)
        {
            env.PlantsData.Clear();

            // Collect and add plant properties to data
            collector.CollectData(
                env.ObjectsCurrentlyInEnv
                .Where(obj => obj is Plant)
                .Select(obj => (Plant)obj)
                .Select(plant => plant.Properties)
            ).ToList().ForEach(obj => env.PlantsData.Add(obj.Key, obj.Value));

            // Same for organism values
            collector.CollectData(
                env.ObjectsCurrentlyInEnv
                .Where(obj => obj is Plant)
                .Select(obj => (Organism)obj)
            ).ToList().ForEach(obj => env.PlantsData.Add(obj.Key, obj.Value));

            // And manager values
            collector.CollectSingleValueData(this).ToList().ForEach(obj => env.PlantsData.Add(obj.Key, obj.Value));

            // Also log the environment values
            collector.CollectSingleValueData(env).ToList().ForEach(obj => env.PlantsData.Add(obj.Key, obj.Value));

            loggers[$"Env{cnt}Plants"].Log(env.PlantsData);
        }

        private void LogEnvAnimals(Objects.Environment env, int cnt) 
        {
            env.AnimalsData.Clear();

            // Collect and add animal properties to data
            collector.CollectData(
                env.ObjectsCurrentlyInEnv
                .Where(obj => obj is Animal)
                .Select(obj => (Animal)obj)
                .Select(animal => animal.Properties)
            ).ToList().ForEach(obj => env.AnimalsData.Add(obj.Key, obj.Value));

            // Same for organism values
            collector.CollectData(
                env.ObjectsCurrentlyInEnv
                .Where(obj => obj is Animal)
                .Select(obj => (Organism)obj)
            ).ToList().ForEach(obj => env.AnimalsData.Add(obj.Key, obj.Value));

            // And manager values
            collector.CollectSingleValueData(this).ToList().ForEach(obj => env.AnimalsData.Add(obj.Key, obj.Value));

            // Also log the environment values
            collector.CollectSingleValueData(env).ToList().ForEach(obj => env.AnimalsData.Add(obj.Key, obj.Value));

            loggers[$"Env{cnt}Animals"].Log(env.AnimalsData);
        }

        private void LogAllPlants()
        {
            PlantsData.Clear();

            // Collect plant properties and add them to data
            collector.CollectData(
                SimulationScene.SimulationObjects
                .Where(obj => obj is Plant)
                .Select(obj => (Plant)obj)
                .Select(plant => plant.Properties)
            ).ToList().ForEach(obj => PlantsData.Add(obj.Key, obj.Value));

            // Collect (plant) organism values e.g age and add them to data
            collector.CollectData(
                SimulationScene.SimulationObjects
                .Where(obj => obj is Plant)
                .Select(obj => (Organism)obj)
            ).ToList().ForEach(obj => PlantsData.Add(obj.Key, obj.Value));

            // Collect manager values and add them to data
            collector.CollectSingleValueData(this).ToList().ForEach(obj => PlantsData.Add(obj.Key, obj.Value));

            // Log collected values
            loggers["AllPlants"].Log(PlantsData);
        }

        private void LogAllAnimals()
        {
            AnimalsData.Clear();

            // Collect animal properties and add them to data
            collector.CollectData(
                SimulationScene.SimulationObjects
                .Where(obj => obj is Animal)
                .Select(obj => (Animal)obj)
                .Select(animal => animal.Properties)
            ).ToList().ForEach(obj => AnimalsData.Add(obj.Key, obj.Value));

            // Collect (plant) organism values e.g age and add them to data
            collector.CollectData(
                SimulationScene.SimulationObjects
                .Where(obj => obj is Animal)
                .Select(obj => (Organism)obj)
            ).ToList().ForEach(obj => AnimalsData.Add(obj.Key, obj.Value));

            // Collect manager values and add them to data
            collector.CollectSingleValueData(this).ToList().ForEach(obj => AnimalsData.Add(obj.Key, obj.Value));

            // Log collected values
            loggers["AllAnimals"].Log(AnimalsData);
        }

        public void SetUpLogging() 
        {
            Directory.CreateDirectory(SimulationSettings.Instance.SimulationLogDirectory);

            // Get all titles of the values that will be logged
            List<string> plantTitles = collector.GetTypeDataPointTitles(typeof(PlantProperties));
            List<string> animalTitles = collector.GetTypeDataPointTitles(typeof(AnimalProperties));
            List<string> commonTitles = collector.GetTypeDataPointTitles(typeof(Organism));
            commonTitles.AddRange(collector.GetTypeDataPointTitles(typeof(Manager)));
            List<string> envTitles = collector.GetTypeDataPointTitles(typeof(Objects.Environment));

            List<string> addedTitles = new List<string>();
            addedTitles.AddRange(plantTitles);
            addedTitles.AddRange(commonTitles);

            loggers["AllPlants"] = new FileLogger(Path.Combine(SimulationSettings.Instance.SimulationLogDirectory, "all-plants"), addedTitles.ToArray());

            addedTitles.AddRange(envTitles);

            for(int i = 1; i <= Envs.Length; i++)
            {
                loggers[$"Env{i}Plants"] = new FileLogger(Path.Combine(SimulationSettings.Instance.SimulationLogDirectory, $"env{i}-plants"), addedTitles.ToArray());
            }

            // Clear added titles for animals
            addedTitles.Clear();
            addedTitles.AddRange(commonTitles);
            addedTitles.AddRange(animalTitles);

            loggers["AllAnimals"] = new FileLogger(Path.Combine(SimulationSettings.Instance.SimulationLogDirectory, "all-animals"), addedTitles.ToArray());

            addedTitles.AddRange(envTitles);

            for (int i = 1; i <= Envs.Length; i++)
            {
                loggers[$"Env{i}Animals"] = new FileLogger(Path.Combine(SimulationSettings.Instance.SimulationLogDirectory, $"env{i}-animals"), addedTitles.ToArray());
            }
        }
    }
}