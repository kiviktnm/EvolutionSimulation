using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windore.Simulations2D;
using Windore.Simulations2D.Data;
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
            get => 0.5d * CurrentSize
                + Properties.Toxicity.Value * CurrentSize
                + Properties.EnvironmentToxicityResistance.Value / 2
                + Properties.TemperatureChangeResistance.Value / 2 * (CurrentSize / 2d)
                + Math.Max(0, Environment.Toxicity.Value - Properties.EnvironmentToxicityResistance.Value)
                + Math.Abs(Properties.OptimalTemperature.Value - Environment.Temperature.Value) / Properties.TemperatureChangeResistance.Value * (CurrentSize / 2d);
        }

        public override double EnergyProduction
        {
            get
            {
                double baseProduction = Math.Abs(Properties.EnergyProductionInLowNutrientGrounds.Value - Environment.GroundNutrientContent.Value) / 3d * CurrentSize;
                double production = baseProduction;

                foreach (Plant plant in GetSimulationObjectsInRange(CurrentSize * 4).Where(obj => obj is Plant)) 
                {
                    // When calculating distance, the size of the plants is taken into account
                    double distance = Position.DistanceTo(plant.Position) - CurrentSize / 2d - plant.CurrentSize / 2d;

                    distance = SMath.Clamp(distance, 0.0001d, Math.Abs(distance));

                    production -= baseProduction / 40d / distance;
                }

                return production;
            }
        }

        public Plant(Point position, double startingEnergy, PlantProperties properties) : base(new Shape(position, 1, 1, true), new Color(0, (byte)(255 - (155 * properties.Toxicity.Value / properties.Toxicity.MaxValue)), 0))
        {
            Properties = properties;

            // This rather unoptimized way is used to maximize the stored energy offspring have and minimize their size
            CurrentSize = 1;
            startingEnergy--;

            while (startingEnergy > 0)
            {
                double energy = EnergyStoringCapacity - CurrentEnergy;
                if (energy <= 0)
                {
                    if (startingEnergy > 1)
                    {
                        CurrentSize++;
                        startingEnergy--;
                    }
                    else
                    {
                        CurrentSize += startingEnergy;
                        startingEnergy = 0;
                    }
                }
                else if (energy < startingEnergy)
                {
                    CurrentEnergy += energy;
                    startingEnergy -= energy;
                }
                else
                {
                    CurrentEnergy += startingEnergy;
                    startingEnergy = 0;
                }
            }
        }

        public override void Update()
        {
            BasicUpdate(new Percentage(Properties.BackupEnergy.Value), new Percentage(Properties.ReproductionEnergy.Value));
        }

        protected override void Reproduce()
        {
            CurrentEnergy -= new Percentage(Properties.ReproductionEnergy.Value) * EnergyStoringCapacity;

            // 20% of energy is lost in reproduction
            double actualReproductionEnergy = new Percentage(Properties.ReproductionEnergy.Value) * EnergyStoringCapacity - new Percentage(Properties.ReproductionEnergy.Value) * EnergyStoringCapacity * 0.2d;

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
                // Bruteforce a position within an environment and one that is not within the parent plant
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
