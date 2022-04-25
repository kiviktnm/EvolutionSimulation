using System.Collections.Generic;
using System.Linq;
using Windore.Simulations2D;
using Windore.Simulations2D.Data;
using Windore.Simulations2D.Util.SMath;
using Windore.Simulations2D.Util;

namespace Windore.EvolutionSimulation.Objects
{
    public class Environment : SimulationObject, IDataSource
    {
        private object plantsDataLock = new object();
        private object animalsDataLock = new object();
        private Dictionary<string, DataCollector.Data> plantsData = new Dictionary<string, DataCollector.Data>();
        private Dictionary<string, DataCollector.Data> animalsData = new Dictionary<string, DataCollector.Data>();

        public string Name { get; set; }
        public ChangingVariable Toxicity { get; set; }
        public ChangingVariable Temperature { get; set; }
        public ChangingVariable SoilNutrientContent { get; set; }

        [DataPoint("EnvironmentAnimalAmount")]
        public int AnimalAmount { get; private set; }
        [DataPoint("EnvironmentPlantAmount")]
        public int PlantAmount { get; private set; }

        // These properties exist just for data collection since ChangingVariable cannot be a DataPoint.
        [DataPoint("EnvironmentalToxinContent")]
        public double ToxicityDC { get => Toxicity.Value; }
        [DataPoint("EnvironmentTemperature")]
        public double TemperatureDC { get => Temperature.Value; }
        [DataPoint("EnvironmentSoilNutrientContent")]
        public double SoilNutrientContentDC { get => SoilNutrientContent.Value; }

        public List<SimulationObject> OrganismsCurrentlyInEnv { get; set; } = new List<SimulationObject>();

        public Environment(Point position, double width, double height) : base(new Shape(position, width, height, true), new Color(0, 0, 0)) { }

        public override void Update()
        {
            Toxicity.Update();
            Temperature.Update();
            SoilNutrientContent.Update();

            Color = new Color((byte)(255 * (Temperature.Value / 100)), (byte)(255 * (SoilNutrientContent.Value / 20)), (byte)(255 * (Toxicity.Value / 100)));

            GetOrganismsInEnv();
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

        public void CollectPlantData(DataCollector collector)
        {
            lock (plantsDataLock)
            {
                plantsData.Clear();

                // Collect and add plant properties to data
                collector.CollectData(
                    OrganismsCurrentlyInEnv
                    .Where(obj => obj is Plant)
                    .Select(obj => (Plant)obj)
                    .Select(plant => plant.Properties)
                ).ToList().ForEach(obj => plantsData.Add(obj.Key, obj.Value));

                // Same for organism values
                collector.CollectData(
                    OrganismsCurrentlyInEnv
                    .Where(obj => obj is Plant)
                    .Select(obj => (Organism)obj)
                ).ToList().ForEach(obj => plantsData.Add(obj.Key, obj.Value));

                // And manager values
                collector.CollectSingleValueData(Simulation.Ins.SimulationManager).ToList().ForEach(obj => plantsData.Add(obj.Key, obj.Value));

                // Also log the environment values
                collector.CollectSingleValueData(this).ToList().ForEach(obj => plantsData.Add(obj.Key, obj.Value));
            }
        }

        public void CollectAnimalData(DataCollector collector)
        {
            lock (animalsDataLock)
            {
                animalsData.Clear();

                // Collect and add animal properties to data
                collector.CollectData(
                    OrganismsCurrentlyInEnv
                    .Where(obj => obj is Animal)
                    .Select(obj => (Animal)obj)
                    .Select(animal => animal.Properties)
                ).ToList().ForEach(obj => animalsData.Add(obj.Key, obj.Value));

                // Same for organism values
                collector.CollectData(
                    OrganismsCurrentlyInEnv
                    .Where(obj => obj is Animal)
                    .Select(obj => (Organism)obj)
                ).ToList().ForEach(obj => animalsData.Add(obj.Key, obj.Value));

                // And manager values
                collector.CollectSingleValueData(Simulation.Ins.SimulationManager).ToList().ForEach(obj => animalsData.Add(obj.Key, obj.Value));

                // Also log the environment values
                collector.CollectSingleValueData(this).ToList().ForEach(obj => animalsData.Add(obj.Key, obj.Value));
            }
        }

        private void GetOrganismsInEnv()
        {
            OrganismsCurrentlyInEnv.Clear();
            AnimalAmount = 0;
            PlantAmount = 0;

            foreach (SimulationObject obj in Scene.SimulationObjects)
            {
                if (obj is Organism org)
                {
                    if (org.OverlappingWith(this) && org.Environment == this)
                    {
                        OrganismsCurrentlyInEnv.Add(org);
                        if (org is Animal) AnimalAmount++;
                        else if (org is Plant) PlantAmount++;
                    }
                }
            }
        }
    }
}