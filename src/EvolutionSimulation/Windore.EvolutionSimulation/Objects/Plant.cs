﻿using System;
using System.Linq;
using Windore.Simulations2D.Util;
using Windore.Simulations2D.Util.SMath;

namespace Windore.EvolutionSimulation.Objects
{
    public class Plant : Organism
    {
        public override Environment Environment => Manager.GetEnvironment(Position);
        public PlantProperties Properties { get; }

        public override double EnergyConsumption
        {
            get => Simulation.ENERGY_COEFFICIENT * (0.5d * CurrentSize
                + Properties.Toxicity.Value * CurrentSize
                + Properties.EnvironmentalToxinResistance.Value / 2
                + Properties.TemperatureChangeResistance.Value / 2 * (CurrentSize / 2d)
                + Math.Max(0, Environment.Toxicity.Value - Properties.EnvironmentalToxinResistance.Value)
                + Properties.EnergyProductionInLowNutrientSoil.Value
                + Math.Abs(Properties.OptimalTemperature.Value - Environment.Temperature.Value) / Properties.TemperatureChangeResistance.Value * (CurrentSize / 2d));
        }

        public override double EnergyProduction
        {
            get
            {
                double baseProduction = Math.Max(Properties.EnergyProductionInLowNutrientSoil.Value, Environment.SoilNutrientContent.Value) * Math.Pow(2d / 7d * CurrentSize, 2);
                double production = baseProduction;

                foreach (Plant plant in GetSimulationObjectsInRange(CurrentSize * 2).Where(obj => obj is Plant))
                {
                    // When calculating distance, the size of the plants is taken into account
                    double distance = Position.DistanceTo(plant.Position) - CurrentSize / 2d - plant.CurrentSize / 2d;

                    distance = SMath.Clamp(distance, 0.0001d, Math.Abs(distance));

                    production -= baseProduction / 20d / distance;
                }

                return Simulation.ENERGY_COEFFICIENT * (production);
            }
        }

        public Plant(Point position, double startingEnergy, double startingSize, PlantProperties properties) : base(new Shape(position, 1, 1, true),
                    new Color(0, (byte)(255 - (155 * properties.Toxicity.Value / properties.Toxicity.MaxValue)), 0),
                    properties)
        {
            Properties = properties;

            CurrentSize = startingSize;
            CurrentEnergy = startingEnergy;
        }

        public override void Update()
        {
            BasicUpdate();
        }

        protected override void Reproduce()
        {
            double reproEnergy = new Percentage(Properties.ReproductionEnergy.Value) * EnergyStoringCapacity;
            CurrentEnergy -= reproEnergy;

            int offspringAmount = GetOffspringAmount();

            double energyForOffspring = reproEnergy / offspringAmount;

            for(int i = 0; i < offspringAmount; i++)
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
                double placingEnergy = (Position.DistanceTo(pos) - CurrentSize / 2d) * Simulation.ENERGY_COEFFICIENT / 2d;
                Plant offspring = new Plant(pos, energyForOffspring * 2d / 3d - placingEnergy, energyForOffspring * 1d / 3d, Properties.CreateMutated());
                offspring.Generation = Generation + 1;

                Scene.Add(offspring);
            }
        }
    }
}
