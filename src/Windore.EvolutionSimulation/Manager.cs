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
        public ChangingVariable BaseEnvToxicity { get; } = new ChangingVariable(Simulation.Ins.Settings.BaseEnvToxicity, 0, 100, Simulation.Ins.Settings.BaseEnvToxicityCPU) { ShouldReverse = Simulation.Ins.Settings.BaseEnvReverseChanging };
        public ChangingVariable BaseEnvTemperature { get; } = new ChangingVariable(Simulation.Ins.Settings.BaseEnvTemperature, 0, 100, Simulation.Ins.Settings.BaseEnvTemperatureCPU) { ShouldReverse = Simulation.Ins.Settings.BaseEnvReverseChanging };
        public ChangingVariable BaseEnvGroundNutrientContent { get; } = new ChangingVariable(Simulation.Ins.Settings.BaseEnvGroundNutrientContent, 0, 20, Simulation.Ins.Settings.BaseEnvGroundNutrientContentCPU) { ShouldReverse = Simulation.Ins.Settings.BaseEnvReverseChanging };

        public Objects.Environment[] Envs { get; set; } = new Objects.Environment[3];

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

        public Manager(SimulationScene scene) : base(scene) { }

        public Objects.Environment GetEnvironment(Point position)
        {
            foreach (Objects.Environment env in Envs)
            {
                Shape testShape = new Shape(position, 1, 1, true);
                if (testShape.Overlaps(env.Shape))
                {
                    return env;
                }
            }
            return null;
        }

        public void SetUpLogging()
        {
            Directory.CreateDirectory(Simulation.Ins.Settings.SimulationLogDirectory);

            // Get all titles of the values that will be logged
            List<string> plantTitles = collector.GetTypeDataPointTitles(typeof(PlantProperties));
            List<string> animalTitles = collector.GetTypeDataPointTitles(typeof(AnimalProperties));
            List<string> commonTitles = collector.GetTypeDataPointTitles(typeof(Organism));
            commonTitles.AddRange(collector.GetTypeDataPointTitles(typeof(Manager)));
            List<string> envTitles = collector.GetTypeDataPointTitles(typeof(Objects.Environment));

            List<string> addedPlantTitles = new List<string>();
            List<string> addedAnimalTitles = new List<string>();

            // Add common titles to both
            addedPlantTitles.AddRange(commonTitles);
            addedAnimalTitles.AddRange(commonTitles);

            // Add specific titles
            addedPlantTitles.AddRange(plantTitles);
            addedAnimalTitles.AddRange(animalTitles);

            // Create loggers for all animals and plants
            loggers["AllPlants"] = new FileLogger(Path.Combine(Simulation.Ins.Settings.SimulationLogDirectory, "all-plants"), addedPlantTitles.ToArray());
            loggers["AllAnimals"] = new FileLogger(Path.Combine(Simulation.Ins.Settings.SimulationLogDirectory, "all-animals"), addedAnimalTitles.ToArray());

            // Add env titles to both
            addedPlantTitles.AddRange(envTitles);
            addedAnimalTitles.AddRange(envTitles);

            for (int i = 0; i < Envs.Length; i++)
            {
                loggers[$"Env{i}Plants"] = new FileLogger(Path.Combine(Simulation.Ins.Settings.SimulationLogDirectory, $"env{i}-plants"), addedPlantTitles.ToArray());
                loggers[$"Env{i}Animals"] = new FileLogger(Path.Combine(Simulation.Ins.Settings.SimulationLogDirectory, $"env{i}-animals"), addedAnimalTitles.ToArray());
            }
        }

        public override void BeforeUpdate()
        {
            BaseEnvToxicity.Update();
            BaseEnvTemperature.Update();
            BaseEnvGroundNutrientContent.Update();
        }

        public override void AfterUpdate()
        {
            if (SimulationScene.Age % 70 == 0)
            {
                Log();
            }
        }

        private void Log()
        {
            if (loggers.Count == 0)
            {
                throw new Exception("Cannot log data because logging has not been set up yet.");
            }

            LogAllAnimals();
            LogAllPlants();

            for (int i = 0; i < Envs.Length; i++)
            {
                LogEnvPlants(i);
                LogEnvAnimals(i);
            }
        }

        private void LogEnvPlants(int envNumber)
        {
            Objects.Environment env = Envs[envNumber];

            env.PlantsData.Clear();

            // Collect and add plant properties to data
            collector.CollectData(
                env.OrganismsCurrentlyInEnv
                .Where(obj => obj is Plant)
                .Select(obj => (Plant)obj)
                .Select(plant => plant.Properties)
            ).ToList().ForEach(obj => env.PlantsData.Add(obj.Key, obj.Value));

            // Same for organism values
            collector.CollectData(
                env.OrganismsCurrentlyInEnv
                .Where(obj => obj is Plant)
                .Select(obj => (Organism)obj)
            ).ToList().ForEach(obj => env.PlantsData.Add(obj.Key, obj.Value));

            // And manager values
            collector.CollectSingleValueData(this).ToList().ForEach(obj => env.PlantsData.Add(obj.Key, obj.Value));

            // Also log the environment values
            collector.CollectSingleValueData(env).ToList().ForEach(obj => env.PlantsData.Add(obj.Key, obj.Value));

            loggers[$"Env{envNumber}Plants"].Log(env.PlantsData);
        }

        private void LogEnvAnimals(int envNumber)
        {
            Objects.Environment env = Envs[envNumber];

            env.AnimalsData.Clear();

            // Collect and add animal properties to data
            collector.CollectData(
                env.OrganismsCurrentlyInEnv
                .Where(obj => obj is Animal)
                .Select(obj => (Animal)obj)
                .Select(animal => animal.Properties)
            ).ToList().ForEach(obj => env.AnimalsData.Add(obj.Key, obj.Value));

            // Same for organism values
            collector.CollectData(
                env.OrganismsCurrentlyInEnv
                .Where(obj => obj is Animal)
                .Select(obj => (Organism)obj)
            ).ToList().ForEach(obj => env.AnimalsData.Add(obj.Key, obj.Value));

            // And manager values
            collector.CollectSingleValueData(this).ToList().ForEach(obj => env.AnimalsData.Add(obj.Key, obj.Value));

            // Also log the environment values
            collector.CollectSingleValueData(env).ToList().ForEach(obj => env.AnimalsData.Add(obj.Key, obj.Value));

            loggers[$"Env{envNumber}Animals"].Log(env.AnimalsData);
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
    }
}