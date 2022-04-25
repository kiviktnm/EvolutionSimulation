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
    public class Manager : SimulationManager, IDataSource
    {
        private object plantsDataLock = new object();
        private object animalsDataLock = new object();
        private Dictionary<string, DataCollector.Data> plantsData = new Dictionary<string, DataCollector.Data>();
        private Dictionary<string, DataCollector.Data> animalsData = new Dictionary<string, DataCollector.Data>();

        public ChangingVariable BaseEnvToxicity { get; } = new ChangingVariable(Simulation.Ins.Settings.BaseEnvEnvironmentalToxinContent, 0, 100, Simulation.Ins.Settings.BaseEnvEnvironmentalToxinContentCPU) { ShouldReverse = Simulation.Ins.Settings.BaseEnvReverseChanging };
        public ChangingVariable BaseEnvTemperature { get; } = new ChangingVariable(Simulation.Ins.Settings.BaseEnvTemperature, 0, 100, Simulation.Ins.Settings.BaseEnvTemperatureCPU) { ShouldReverse = Simulation.Ins.Settings.BaseEnvReverseChanging };
        public ChangingVariable BaseEnvSoilNutrientContent { get; } = new ChangingVariable(Simulation.Ins.Settings.BaseEnvSoilNutrientContent, 0, 20, Simulation.Ins.Settings.BaseEnvSoilNutrientContentCPU) { ShouldReverse = Simulation.Ins.Settings.BaseEnvReverseChanging };

        public Objects.Environment[] Envs { get; set; } = new Objects.Environment[3];

        [DataPoint("Duration")]
        public ulong Duration { get => SimulationScene.Age; }
        [DataPoint("PlantAmount")]
        public int PlantAmount { get => SimulationScene.SimulationObjects.Where(obj => obj is Plant).Count(); }
        [DataPoint("AnimalAmount")]
        public int AnimalAmount { get => SimulationScene.SimulationObjects.Where(obj => obj is Animal).Count(); }

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

            foreach(Objects.Environment env in Envs)
            {
                loggers[$"Env-{env.Name}-Plants"] = new FileLogger(Path.Combine(Simulation.Ins.Settings.SimulationLogDirectory, $"env-{env.Name}-plants"), addedPlantTitles.ToArray());
                loggers[$"Env-{env.Name}-Animals"] = new FileLogger(Path.Combine(Simulation.Ins.Settings.SimulationLogDirectory, $"env-{env.Name}-animals"), addedAnimalTitles.ToArray());
            }
        }

        public override void BeforeUpdate()
        {
            BaseEnvToxicity.Update();
            BaseEnvTemperature.Update();
            BaseEnvSoilNutrientContent.Update();
        }

        public override void AfterUpdate()
        {
            if (SimulationScene.Age % 70 == 0)
            {
                Log();
            }
        }

        public Dictionary<string, DataCollector.Data> GetData(DataType type)
        {
            switch (type)
            {
                case DataType.Animal:
                    lock (animalsDataLock)
                    {
                        Dictionary<string, DataCollector.Data> data = new Dictionary<string, DataCollector.Data>();
                        foreach (string key in animalsData.Keys)
                            data.Add(key, animalsData[key].DeepCopy());
                        return data;
                    }

                case DataType.Plant:
                    lock (plantsDataLock)
                    {
                        Dictionary<string, DataCollector.Data> pData = new Dictionary<string, DataCollector.Data>();
                        foreach (string key in plantsData.Keys)
                            pData.Add(key, plantsData[key].DeepCopy());
                        return pData;
                    }
                default:
                    return new Dictionary<string, DataCollector.Data>();
            }
        }

        private void Log()
        {
            if (loggers.Count == 0)
            {
                throw new Exception("Cannot log data because logging has not been set up yet.");
            }

            lock (animalsDataLock)
                LogAllAnimals();
                
            lock (plantsDataLock)
                LogAllPlants();

            foreach (Objects.Environment env in Envs)
            {
                env.CollectPlantData(collector);
                loggers[$"Env-{env.Name}-Plants"].Log(env.GetData(DataType.Plant));
        
                env.CollectAnimalData(collector);
                loggers[$"Env-{env.Name}-Animals"].Log(env.GetData(DataType.Animal));
            }
        }

        private void LogAllPlants()
        {
            plantsData.Clear();

            // Collect plant properties and add them to data
            collector.CollectData(
                SimulationScene.SimulationObjects
                .Where(obj => obj is Plant)
                .Select(obj => (Plant)obj)
                .Select(plant => plant.Properties)
            ).ToList().ForEach(obj => plantsData.Add(obj.Key, obj.Value));

            // Collect (plant) organism values e.g age and add them to data
            collector.CollectData(
                SimulationScene.SimulationObjects
                .Where(obj => obj is Plant)
                .Select(obj => (Organism)obj)
            ).ToList().ForEach(obj => plantsData.Add(obj.Key, obj.Value));

            // Collect manager values and add them to data
            collector.CollectSingleValueData(this).ToList().ForEach(obj => plantsData.Add(obj.Key, obj.Value));

            // Log collected values
            loggers["AllPlants"].Log(plantsData);
        }

        private void LogAllAnimals()
        {
            animalsData.Clear();

            // Collect animal properties and add them to data
            collector.CollectData(
                SimulationScene.SimulationObjects
                .Where(obj => obj is Animal)
                .Select(obj => (Animal)obj)
                .Select(animal => animal.Properties)
            ).ToList().ForEach(obj => animalsData.Add(obj.Key, obj.Value));

            // Collect (plant) organism values e.g age and add them to data
            collector.CollectData(
                SimulationScene.SimulationObjects
                .Where(obj => obj is Animal)
                .Select(obj => (Organism)obj)
            ).ToList().ForEach(obj => animalsData.Add(obj.Key, obj.Value));

            // Collect manager values and add them to data
            collector.CollectSingleValueData(this).ToList().ForEach(obj => animalsData.Add(obj.Key, obj.Value));

            // Log collected values
            loggers["AllAnimals"].Log(animalsData);
        }
    }
}