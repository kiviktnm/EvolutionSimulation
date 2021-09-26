using System;

namespace Windore.EvolutionSimulation
{
    public class ChangingVariable
    {
        private readonly ChangingVariable ParentChangingVariable;
        private readonly bool hasParent;
        private double diffFromParent = 0;
        public double Value
        {
            get
            {
                if (hasParent) return ParentChangingVariable.Value + diffFromParent;
                else return diffFromParent;
            }
            set
            {
                // Set the diff to be equal the number that should be added to parent's value to reach the set value
                if (hasParent) diffFromParent = value - ParentChangingVariable.Value;
                else diffFromParent = value;
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
            hasParent = false;
        }

        public ChangingVariable(double diffFromParent, double minAbsDiffFromParent, double maxAbsDiffFromParent, double changePerUpdate, ChangingVariable ParentValue)
        {
            this.diffFromParent = diffFromParent;
            MaxValue = maxAbsDiffFromParent;
            MinValue = minAbsDiffFromParent;

            if (MaxValue < 0)
                throw new ArgumentException("Maximum absolute difference cannot be less than zero.");
            if (MinValue < 0)
                throw new ArgumentException("Minimum absolute difference cannot be less than zero.");

            ChangePerUpdate = changePerUpdate;
            ParentChangingVariable = ParentValue;
            hasParent = true;
        }

        public void Update()
        {
            diffFromParent += ChangePerUpdate;
            if (hasParent)
            {
                HasParentUpdateChecks();
            }
            else
            {
                DefaultUpdateChecks();
            }
        }

        private void HasParentUpdateChecks()
        {
            // If absolute difference from Parent is greater than max absolute difference from Parent swap number reverse
            if (Math.Abs(diffFromParent) > MaxValue)
            {
                if (diffFromParent > 0)
                    diffFromParent = MaxValue;
                else
                    diffFromParent = -MaxValue;

                Reverse();
            }

            // Same but for mininum absolute difference
            if (Math.Abs(diffFromParent) < MinValue)
            {
                if (diffFromParent > 0)
                    diffFromParent = -MinValue;
                else
                    diffFromParent = MinValue;
            }

            // A child changing variable cannot have greater min or max values than its parent, but it should not reverse its change in this case
            if (Value > ParentChangingVariable.MaxValue)
                Value = ParentChangingVariable.MaxValue;

            if (Value < ParentChangingVariable.MinValue)
                Value = ParentChangingVariable.MinValue;
        }

        private void DefaultUpdateChecks()
        {
            // Reverse changing direction if max or min value is reached
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