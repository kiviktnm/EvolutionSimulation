using Windore.Simulations2D.Util.SMath;
using Windore.Simulations2D.Util;
using Windore.Simulations2D;
using System.Collections.Generic;
using System.Linq;
using Windore.Simulations2D.Data;

namespace Windore.EvolutionSimulation.Objects
{
    public class Environment : SimulationObject
    {
        public string Name { get; set; }
        public ChangingVariable Toxicity { get; set; }
        public ChangingVariable Temperature { get; set; }
        public ChangingVariable GroundNutrientContent { get; set; }

        [DataPoint("EnvironvementAnimalAmount")]
        public int AnimalAmount { get => 0; }
        [DataPoint("EnvironvementPlantAmount")]
        public int PlantAmount { get => ObjectsCurrentlyInEnv.Where(obj => obj is Plant).Count(); }

        // These properties exist just for data collection.
        [DataPoint("EnvironvementToxicity")]
        public double ToxicityDC { get => Toxicity.Value; }
        [DataPoint("EnvironvementTemperature")]
        public double TemperatureDC { get => Temperature.Value; }
        [DataPoint("EnvironvementGroundNutrientContent")]
        public double GroundNutrientContentDC { get => GroundNutrientContent.Value; }

        public Dictionary<string, DataCollector.Data> PlantsData { get; set; } = new Dictionary<string, DataCollector.Data>();

        public List<SimulationObject> ObjectsCurrentlyInEnv { get; set; } = new List<SimulationObject>();

        public Environment(Point position, double size) : base(new Shape(position , size, size, true), new Color(0, 0, 0)){}

        public override void Update()  
        {
            Toxicity.Update();
            Temperature.Update();
            GroundNutrientContent.Update();

            Color = new Color((byte)(255 * (Temperature.Value / 100)), (byte)(255 * (GroundNutrientContent.Value / 20)), (byte)(255 * (Toxicity.Value / 100)));
            ObjectsCurrentlyInEnv = GetSimulationObjectsInRange(Shape.Width / 2d);
        }
    }
}