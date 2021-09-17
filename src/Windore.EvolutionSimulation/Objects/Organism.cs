using System;
using Windore.Simulations2D;
using Windore.Simulations2D.Data;
using Windore.Simulations2D.Util;
using Windore.Simulations2D.Util.SMath;

namespace Windore.EvolutionSimulation.Objects
{
    public abstract class Organism : SimulationObject
    {
        protected Manager Manager => SimulationSettings.Instance.SimulationManager;
        protected SRandom Random => SimulationSettings.Instance.SimulationRandom;
        private double energy = 0;
        private double size = 0;

        protected Organism(Shape shape, Color color) : base(shape, color)
        {
        }

        public double CurrentSize 
        {
            get => size;
            set
            {
                size = Math.Min(MaxSize, value);
                Shape = new Shape(Position, size, size, Shape.IsEllipse);
            }
        }
        public double CurrentEnergy 
        {
            get => energy;
            set
            {
                energy = Math.Min(EnergyStoringCapacity, value);

                // If the organism runs out of energy it dies o7
                if (energy < 0)
                {
                    energy = 0;
                    Remove();
                }
            }
        }
        public double EnergyStoringCapacity { get => CurrentSize * 3; }
        public abstract double MaxSize { get; }
        public abstract double EnergyConsumption { get; }
        public abstract double EnergyProduction { get; }

        [DataPoint("Age")]
        public int Age { get; private set; } = 1;

        protected abstract void Reproduce();
        protected void BasicUpdate(double backupEnergy, double reproductionEnergy) 
        {
            Age++;
            CurrentEnergy += EnergyProduction - EnergyConsumption;
            double extraEnergy = CurrentEnergy - (EnergyStoringCapacity * backupEnergy);

            if (extraEnergy > 0) 
            {
                extraEnergy = Grow(extraEnergy);
                if (extraEnergy > EnergyStoringCapacity * reproductionEnergy && CurrentSize >= MaxSize) 
                {
                    Reproduce();
                }
            }
        }

        // Returns extra energy left over
        private double Grow(double extraEnergy) 
        {
            if (CurrentSize >= MaxSize) return extraEnergy;

            double growth = Math.Min(extraEnergy, MaxSize - CurrentSize);
            CurrentSize += growth;
            CurrentEnergy -= growth;
            return extraEnergy - growth;
        }
    }
}