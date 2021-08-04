using System;

namespace Windore.EvolutionSimulation
{
    public class ChangingVariable
    {
        private readonly ChangingVariable baseChangingVariable;
        private readonly bool hasBase;
        private double diffFromBase = 0;
        public double Value
        { 
            get 
            {
                if (hasBase) return baseChangingVariable.Value + diffFromBase;
                else return diffFromBase;
            }
            set 
            {
                if (hasBase) diffFromBase = baseChangingVariable.Value - value;
                else diffFromBase = value;
            }
        }
        public double MaxValue { get; set; } = 0;
        public double MinValue { get; set; } = 0;
        public double ChangePerUpdate { get; set; } = 0;
        public bool ShouldReverse { get; set; } = true;

        public ChangingVariable(double value, double min, double max, double changePerUpdate) 
        {
            Value = value;
            MaxValue = max;
            MinValue = min;
            ChangePerUpdate = changePerUpdate;
            hasBase = false;
        }

        public ChangingVariable(double diffFromBase, double minAbsDiffFromBase, double maxAbsDiffFromBase, double changePerUpdate, ChangingVariable baseValue)
        {
            this.diffFromBase = diffFromBase; 
            MaxValue = maxAbsDiffFromBase;
            MinValue = minAbsDiffFromBase;

            if (MaxValue < 0) 
                throw new ArgumentException("Maximum absolute difference cannot be less than zero.");
            if (MinValue < 0)
                throw new ArgumentException("Minimum absolute difference cannot be less than zero.");

            ChangePerUpdate = changePerUpdate;
            baseChangingVariable = baseValue;
            hasBase = true;
        }

        public void Update() 
        {
            diffFromBase += ChangePerUpdate;
            if (hasBase) 
            {
                HasBaseUpdateChecks();
            }
            else 
            {
                DefaultUpdateChecks();
            }
        }

        private void HasBaseUpdateChecks() 
        {
            if (Math.Abs(diffFromBase) > MaxValue) 
            {
                if (diffFromBase > 0)
                    diffFromBase = MaxValue;
                else
                    diffFromBase = -MaxValue;

                Reverse();
            }
            if (Math.Abs(diffFromBase) < MinValue) 
            {
                // Swap number "polarity" when min diff is reached
                if (diffFromBase > 0)
                    diffFromBase = -MinValue;
                else
                    diffFromBase = MinValue;
            }

            if (Value > baseChangingVariable.MaxValue) 
                Value = baseChangingVariable.MaxValue;

            if (Value < baseChangingVariable.MinValue) 
                Value = baseChangingVariable.MinValue;
        }

        private void DefaultUpdateChecks() 
        {
            if (Value > MaxValue)
            {
                Value = MaxValue;
                Reverse();
            }
            if (Value < MinValue)
            {
                Value = MinValue;
                Reverse();
            }
        }

        private void Reverse() 
        {
            if (ShouldReverse)
                ChangePerUpdate = -ChangePerUpdate;
        }
    }
}