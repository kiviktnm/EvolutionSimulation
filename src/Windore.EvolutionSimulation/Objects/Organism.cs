using System;
using Windore.Simulations2D;
using Windore.Simulations2D.Data;
using Windore.Simulations2D.Util;
using Windore.Simulations2D.Util.SMath;

namespace Windore.EvolutionSimulation.Objects
{
    public abstract class Organism : SimulationObject
    {
        public const double ENERGY_STORING_COEFFICIENT = 2d;
        public const double GROWTH_LIMIT = 2d;

        protected Manager Manager => Simulation.Ins.SimulationManager;
        protected SRandom Random => Simulation.Ins.SimulationRandom;
        private double energy = 0;
        private double size = 0;
        private OrganismProperties properties;

        public double CurrentSize
        {
            get => size;
            set
            {
                size = Math.Min(properties.AdultSize.Value, value);
                Shape = new Shape(Position, size, size, Shape.IsEllipse);
            }
        }

        public double CurrentEnergy
        {
            get => energy;
            set
            {
                energy = Math.Min(EnergyStoringCapacity, value);

                // If the organism runs out of energy it dies
                if (energy < 0)
                {
                    energy = 0;
                    Remove();
                }
            }
        }
        public double EnergyStoringCapacity { get => CurrentSize * ENERGY_STORING_COEFFICIENT; }
        public abstract double EnergyConsumption { get; }
        public abstract double EnergyProduction { get; }

        [DataPoint("Age")]
        public int Age { get; private set; } = 1;

        [DataPoint("Generation")]
        public int Generation { get; set; } = 0;

        protected Organism(Shape shape, Color color, OrganismProperties properties) : base(shape, color)
        {
            this.properties = properties;
        }

        protected void BasicUpdate()
        {
            Age++;
            CurrentEnergy += EnergyProduction - EnergyConsumption;
            double extraEnergy = CurrentEnergy - (EnergyStoringCapacity * new Percentage(properties.BackupEnergy.Value));

            if (extraEnergy > 0)
            {
                if (CurrentSize < properties.AdultSize.Value)
                {
                    extraEnergy -= Grow(extraEnergy);
                }
                if (extraEnergy > EnergyStoringCapacity * new Percentage(properties.ReproductionEnergy.Value))
                {
                    Reproduce();
                }
            }
        }

        protected abstract void Reproduce();

        protected int GetOffspringAmount()
        {
            int actualOffspringAmount = 0;
            int integerOffspringAmount = (int)Math.Floor(properties.OffspringAmount.Value);

            Percentage percentageForAdditionalOffspring = Percentage.FromDouble(properties.OffspringAmount.Value - integerOffspringAmount);
            if (Simulation.Ins.SimulationRandom.Boolean(percentageForAdditionalOffspring))
                actualOffspringAmount++;

            return actualOffspringAmount + integerOffspringAmount;
        }

        // Returns energy spent on growing
        private double Grow(double extraEnergy)
        {
            double growth = Math.Min(extraEnergy, GROWTH_LIMIT);
            growth = Math.Min(growth, properties.AdultSize.Value - CurrentSize);
            CurrentSize += growth;
            CurrentEnergy -= growth;
            return growth;
        }
    }
}