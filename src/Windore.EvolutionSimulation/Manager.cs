using Windore.Simulations2D;
using Windore.Simulations2D.Util.SMath;
using Windore.EvolutionSimulation.Objects;
using System;
using Windore.Simulations2D.Data;
using System.Linq;
using Avalonia.Controls;
using System.Collections.Generic;
using System.IO;
using Windore.Simulations2D.GUI;
using Avalonia.Threading;
using Avalonia.Controls.ApplicationLifetimes;

namespace Windore.EvolutionSimulation 
{
    public class Manager : SimulationManager
    {
        private ChangingVariable baseEnvToxicity = new ChangingVariable(0, 0, 100, 0);
        private ChangingVariable baseEnvTemperature = new ChangingVariable(30, 0, 100, 0);
        private ChangingVariable baseEnvGroundNutrientContent = new ChangingVariable(10, 0, 20, 0);

        public Objects.Environment Env1 { get; private set; }
        public Objects.Environment Env2 { get; private set; }
        public Objects.Environment Env3 { get; private set; }

        public DataWindowManager DataWindowManager { get; } = new DataWindowManager();

        private Objects.Environment[] Envs => new Objects.Environment[]{ Env1, Env2, Env3 };

        [DataPoint("Duration")]
        public ulong Duration { get => SimulationScene.Age; }

        [DataPoint("PlantAmount")]
        public int PlantAmount { get => SimulationScene.SimulationObjects.Where(obj => obj is Plant).Count(); }

        public Dictionary<string, DataCollector.Data> PlantsData { get; private set; } = new Dictionary<string, DataCollector.Data>();

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
                        panel.Children.Add(NewSelectedPanelText($"Current Energy: {Math.Round(plnt.CurrentEnergy, 3)}"));
                        panel.Children.Add(NewSelectedPanelText($"Energy Production: {Math.Round(plnt.EnergyProduction, 3)}"));
                        panel.Children.Add(NewSelectedPanelText($"Energy Consumption: {Math.Round(plnt.EnergyConsumption, 3)}"));
                        panel.Children.Add(NewSelectedPanelText($"Energy Storing Capacity: {Math.Round(plnt.EnergyStoringCapacity, 3)}"));
                        panel.Children.Add(NewSelectedPanelText($"Current Size: {Math.Round(plnt.CurrentSize, 3)}"));
                        panel.Children.Add(NewSelectedPanelText($"Current Age: {plnt.Age}"));

                        foreach (KeyValuePair<string, Property> pair in plnt.Properties.Properties)
                        {
                            panel.Children.Add(NewSelectedPanelText($"{pair.Key}: {Math.Round(pair.Value.Value, 3)}"));
                        }
                        break;
                    case Objects.Environment env:
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

                        panel.Children.Add(plantPanelBtn);
                        panel.Children.Add(animalPanelBtn);
                        break;
                }

                return panel;
            }
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
            baseEnvToxicity.Update();
            baseEnvTemperature.Update();
            baseEnvGroundNutrientContent.Update();
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
                Toxicity = new ChangingVariable(0, 0, 0, 0, baseEnvToxicity),
                Temperature = new ChangingVariable(0, 0, 0, 0, baseEnvTemperature),
                GroundNutrientContent = new ChangingVariable(0, 0, 0, 0, baseEnvGroundNutrientContent),
                Name = "Normal"
            };

            Env2 = new Objects.Environment(new Point(SimulationScene.Width * 0.75, SimulationScene.Height * 0.33), envSize)
            {
                Toxicity = new ChangingVariable(0, 0, 0, 0, baseEnvToxicity),
                Temperature = new ChangingVariable(10, 0, 10, 0, baseEnvTemperature),
                GroundNutrientContent = new ChangingVariable(0, 0, 0, 0, baseEnvGroundNutrientContent),
                Name = "Temperature"
                
            };

            Env3 = new Objects.Environment(new Point(SimulationScene.Width * 0.50, SimulationScene.Height * 0.66), envSize)
            {
                Toxicity = new ChangingVariable(0, 0, 0, 0, baseEnvToxicity),
                Temperature = new ChangingVariable(0, 0, 0, 0, baseEnvTemperature),
                GroundNutrientContent = new ChangingVariable(0, 0, 0, 0, baseEnvGroundNutrientContent),
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

            PlantsData.Clear();

            collector.CollectData(
                SimulationScene.SimulationObjects
                .Where(obj => obj is Plant)
                .Select(obj => (Plant)obj)
                .Select(plant => plant.Properties)
            ).ToList().ForEach(obj => PlantsData.Add(obj.Key, obj.Value));

            collector.CollectData(
                SimulationScene.SimulationObjects
                .Where(obj => obj is Plant)
                .Select(obj => (Organism)obj)
            ).ToList().ForEach(obj => PlantsData.Add(obj.Key, obj.Value));

            collector.CollectSingleValueData(this).ToList().ForEach(obj => PlantsData.Add(obj.Key, obj.Value));

            loggers["AllPlants"].Log(PlantsData);

            int cnt = 1;
            foreach(Objects.Environment env in Envs) 
            {
                env.PlantsData.Clear();

                collector.CollectData(
                    env.ObjectsCurrentlyInEnv
                    .Where(obj => obj is Plant)
                    .Select(obj => (Plant)obj)
                    .Select(plant => plant.Properties)
                ).ToList().ForEach(obj => env.PlantsData.Add(obj.Key, obj.Value));

                collector.CollectData(
                    env.ObjectsCurrentlyInEnv
                    .Where(obj => obj is Plant)
                    .Select(obj => (Organism)obj)
                ).ToList().ForEach(obj => env.PlantsData.Add(obj.Key, obj.Value));

                collector.CollectSingleValueData(this).ToList().ForEach(obj => env.PlantsData.Add(obj.Key, obj.Value));
                collector.CollectSingleValueData(env).ToList().ForEach(obj => env.PlantsData.Add(obj.Key, obj.Value));

                loggers[$"Env{cnt}Plants"].Log(env.PlantsData);
                cnt++;
            }
        }

        public void SetUpLogging() 
        {
            Directory.CreateDirectory(SimulationSettings.Instance.SimulationLogDirectory);

            List<string> plantTitles = collector.GetTypeDataPointTitles(typeof(PlantProperties));
            loggers["AllPlants"] = new FileLogger(Path.Combine(SimulationSettings.Instance.SimulationLogDirectory, "all-plants"), plantTitles.ToArray());

            plantTitles.AddRange(collector.GetTypeDataPointTitles(typeof(Objects.Environment)));

            for(int i = 1; i <= Envs.Length; i++)
            {
                loggers[$"Env{i}Plants"] = new FileLogger(Path.Combine(SimulationSettings.Instance.SimulationLogDirectory, $"env{i}-plants"), plantTitles.ToArray());
            }
        }
    }
}