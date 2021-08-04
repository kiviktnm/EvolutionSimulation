using Windore.Simulations2D.Util.SMath;

namespace Windore.EvolutionSimulation
{
    public class Property
    {
        public double Value { get; set; }
        public double MinValue { get; }
        public double MaxValue { get; }
        public double MutationBaseValue { get; }

        public Property(double value, double minValue, double maxValue, double mutationBaseValue) 
        {
            Value = value;
            MinValue = minValue;
            MaxValue = maxValue;
            MutationBaseValue = mutationBaseValue;
        }

        public Property CreateMutated(Percentage mutationStrength) 
        {
            double newValue = Value;
            if (SimulationSettings.Instance.SimulationRandom.Boolean(new Percentage(SimulationSettings.Instance.MutationProbability))) 
            {
                double amount = mutationStrength * MutationBaseValue;
                newValue += SimulationSettings.Instance.SimulationRandom.Double(-amount, amount);
            }

            return new Property(newValue, MinValue, MaxValue, MutationBaseValue);
        }
    }
}