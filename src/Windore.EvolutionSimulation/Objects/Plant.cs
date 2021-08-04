using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windore.Simulations2D;
using Windore.Simulations2D.Util;
using Windore.Simulations2D.Util.SMath;

namespace Windore.EvolutionSimulation.Objects
{
    public class Plant : Organism
    {
        public Environment Environment => Manager.GetEnvironment(Position);
        public PlantProperties Properties { get; }
        public override double MaxSize => Properties.AdultSize.Value;

        public override double EnergyConsumption
        {
            get => 0.1d * CurrentSize
                + Properties.Toxicity.Value * CurrentSize
                + Properties.EnvironmentToxicityResistance.Value / 2
                + Properties.TemperatureChangeResistance.Value / 2 * CurrentSize
                + Math.Max(0, Environment.Toxicity.Value - Properties.EnvironmentToxicityResistance.Value)
                + Math.Abs(Properties.OptimalTemperature.Value - Environment.Temperature.Value) / Properties.TemperatureChangeResistance.Value * CurrentSize;
        }

        public override double EnergyProduction
        {
            get
            {
                double baseProduction = Math.Abs(Properties.EnergyProductionInLowNutrientGrounds.Value - Environment.GroundNutrientContent.Value) * CurrentSize;
                double production = baseProduction;

                foreach (Plant plant in GetSimulationObjectsInRange(CurrentSize * 4).Where(obj => obj is Plant)) 
                {
                    // When calculating distance, the size of the plants is taken into account
                    double distance = Position.DistanceTo(plant.Position) - CurrentSize / 2d - plant.CurrentSize / 2d;

                    distance = SMath.Clamp(distance, 0.0001d, Math.Abs(distance));

                    production -= baseProduction / 20d / distance;
                }

                return production;
            }
        }

        public Plant(Point position, double startingEnergy, PlantProperties properties) : base(new Shape(position, 1, 1, true), new Color(0, (byte)(255 - (155 * properties.Toxicity.Value / properties.Toxicity.MaxValue)), 0))
        {
            Properties = properties;

            CurrentSize = startingEnergy / 2;
            CurrentEnergy = startingEnergy / 2;
        }

        public override void Update()
        {
            BasicUpdate(Properties.GrowthRate.Value, Properties.BackupEnergy.Value / 100d, Properties.ReproductionEnergy.Value / 100d);
        }

        protected override void Reproduce()
        {
            CurrentEnergy -= Properties.ReproductionEnergy.Value;

            // 20% of energy is lost in reproduction
            double actualReproductionEnergy = Properties.ReproductionEnergy.Value - Properties.ReproductionEnergy.Value * 0.2;

            // Get the integer part of the OffspringAmount
            int offspringAmount = (int)Math.Floor(Properties.OffspringAmount.Value);

            // Then use the fractional part as a chance of adding an another offspring
            double offspringFrac = Properties.OffspringAmount.Value - offspringAmount;
            if (Random.Boolean(Percentage.FromDouble(offspringFrac)))
                offspringAmount++;

            // Energy is divided among offspring
            double energyForOneOffspring = actualReproductionEnergy / offspringAmount;

            for (int i = 0; i < offspringAmount; i++)
            {
                // Bruteforce a position within an environment
                Point pos;
                do
                {
                    double posX = Random.Double(Position.X - (Properties.ReproductionArea.Value + CurrentSize / 2), Position.X + (Properties.ReproductionArea.Value + CurrentSize / 2));
                    double posY = Random.Double(Position.Y - (Properties.ReproductionArea.Value + CurrentSize / 2), Position.Y + (Properties.ReproductionArea.Value + CurrentSize / 2));

                    pos = new Point(posX, posY);
                } while (Manager.GetEnvironment(pos) == null);

                // Some energy is lost for the offspring to appear farther away
                double placingEnergy = (Position.DistanceTo(pos) - CurrentSize / 2d ) / 4d;

                double energy = energyForOneOffspring - Math.Max(0, placingEnergy);
                Plant offspring = new Plant(pos, energy, Properties.CreateMutated());
                Scene.Add(offspring);
            }
        }
    }
}
