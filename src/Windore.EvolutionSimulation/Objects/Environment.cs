using System.Collections.Generic;
using Windore.Simulations2D;
using Windore.Simulations2D.Data;
using Windore.Simulations2D.Util.SMath;
using Windore.Simulations2D.Util;

namespace Windore.EvolutionSimulation.Objects
{
    public class Environment : SimulationObject
    {
        public string Name { get; set; }
        public ChangingVariable Toxicity { get; set; }
        public ChangingVariable Temperature { get; set; }
        public ChangingVariable GroundNutrientContent { get; set; }

        [DataPoint("EnvironvementAnimalAmount")]
        public int AnimalAmount { get; private set; }
        [DataPoint("EnvironvementPlantAmount")]
        public int PlantAmount { get; private set; }

        // These properties exist just for data collection since ChangingVariable cannot be a DataPoint.
        [DataPoint("EnvironvementToxicity")]
        public double ToxicityDC { get => Toxicity.Value; }
        [DataPoint("EnvironvementTemperature")]
        public double TemperatureDC { get => Temperature.Value; }
        [DataPoint("EnvironvementGroundNutrientContent")]
        public double GroundNutrientContentDC { get => GroundNutrientContent.Value; }

        public Dictionary<string, DataCollector.Data> PlantsData { get; set; } = new Dictionary<string, DataCollector.Data>();
        public Dictionary<string, DataCollector.Data> AnimalsData { get; set; } = new Dictionary<string, DataCollector.Data>();

        public List<SimulationObject> OrganismsCurrentlyInEnv { get; set; } = new List<SimulationObject>();

        public Environment(Point position, double size) : base(new Shape(position, size, size, true), new Color(0, 0, 0)) { }

        public override void Update()
        {
            Toxicity.Update();
            Temperature.Update();
            GroundNutrientContent.Update();

            Color = new Color((byte)(255 * (Temperature.Value / 100)), (byte)(255 * (GroundNutrientContent.Value / 20)), (byte)(255 * (Toxicity.Value / 100)));
            
            GetOrganismsInEnv();
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
                    if (org.OverlappingWith(this))
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