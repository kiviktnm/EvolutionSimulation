using System;
using Windore.Simulations2D;
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
                    Remove();
                }
            }
        }
        public double EnergyStoringCapacity { get => CurrentSize * 10; }
        public abstract double MaxSize { get; }
        public abstract double EnergyConsumption { get; }
        public abstract double EnergyProduction { get; }

        protected abstract void Reproduce();
        protected void BasicUpdate(double growthRate, double backupEnergy, double reproductionEnergy) 
        {
            CurrentEnergy += EnergyProduction - EnergyConsumption;
            double extraEnergy = CurrentEnergy - (EnergyStoringCapacity * backupEnergy);

            if (extraEnergy > 0) 
            {
                extraEnergy = Grow(extraEnergy, growthRate);
                if (extraEnergy > EnergyStoringCapacity * reproductionEnergy && CurrentSize >= MaxSize) 
                {
                    Reproduce();
                }
            }
        }

        // Returns extra energy left over
        private double Grow(double extraEnergy, double growthRate) 
        {
            if (CurrentSize >= MaxSize) return extraEnergy;

            double growth = growthRate * MaxSize;
            if (extraEnergy > growth)
            {
                CurrentEnergy -= growth;
                CurrentSize += growth;

                return extraEnergy - growth;
            }
            else
            {
                CurrentEnergy -= extraEnergy;
                CurrentSize += extraEnergy;

                return 0;
            }
        }
    }
}