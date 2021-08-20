using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windore.Simulations2D.Util;
using Windore.Simulations2D.Util.SMath;

namespace Windore.EvolutionSimulation.Objects
{
    public class Animal : Organism
    {
        public Animal(Point position, double startingEnergy, AnimalProperties properties) : base(new Shape(position, 1, 1, false), 
                  new Color(
                      (byte)(255 - (155 * properties.CarnivorityTendency.Value / properties.CarnivorityTendency.MaxValue)),
                      0,
                      (byte)((255 * properties.CarnivorityTendency.Value / properties.CarnivorityTendency.MaxValue) - 100))
                  )
        {
            Properties = properties;

            CurrentSize = startingEnergy / 2;
            CurrentEnergy = startingEnergy / 2;
        }

        public Environment Environment => Manager.GetEnvironment(Position);
        public AnimalProperties Properties { get; }
        public override double MaxSize => Properties.AdultSize.Value;

        private List<Animal> Relatives = new List<Animal>();

        public double MaxInjuries => CurrentSize / 3d;
        private double injs = 0;
        public double Injuries
        {
            get => injs;
            set
            {
                injs = Math.Max(value, 0);
                // A too injured animal dies
                if (injs > MaxInjuries) Remove();
            }
        }

        public override double EnergyConsumption
        {
            get => 0.5d * CurrentSize
                + Properties.MovementSpeed.Value / 2d
                + Properties.Eyesight.Value / 4d
                + Properties.OffensiveCapability.Value * CurrentSize / 3d
                + Properties.DefensiveCapability.Value * CurrentSize / 3d
                + Properties.PlantToxicityResistance.Value / 2d
                + Properties.FoodStoringAndDigestingCapability.Value * CurrentSize / 10d
                + Properties.EnvironmentToxicityResistance.Value / 2
                + Properties.TemperatureChangeResistance.Value / 2 * (CurrentSize / 2d)
                + Math.Max(0, Environment.Toxicity.Value - Properties.EnvironmentToxicityResistance.Value)
                + Math.Abs(Properties.OptimalTemperature.Value - Environment.Temperature.Value) / Properties.TemperatureChangeResistance.Value * (CurrentSize / 2d)
                + Injuries;
        }

        private double FoodStoringCapacity => new Percentage(Properties.FoodStoringAndDigestingCapability.Value) * CurrentSize;
        private double storedFood = 0;
        public double StoredFood
        {
            get => storedFood;
            set
            {
                storedFood = Math.Min(value, FoodStoringCapacity);
            }
        }
        public override double EnergyProduction
        {
            get
            {
                if (StoredFood <= 0) return 0;

                double amountDigested = new Percentage(Properties.FoodStoringAndDigestingCapability.Value) / 10d * new Percentage(Properties.FoodStoringAndDigestingCapability.Value) * CurrentSize;
                if (amountDigested > StoredFood) 
                {
                    double r = StoredFood;
                    StoredFood = 0;
                    return r;
                }

                StoredFood -= amountDigested;
                return amountDigested;
            }
        }

        // Returns the amount eaten from an organism
        private double Eat(Organism organism, Percentage energyGainEfficiency)
        {
            double foodToBeEaten = (FoodStoringCapacity - StoredFood) / energyGainEfficiency;

            if (organism.CurrentEnergy < foodToBeEaten) 
            {
                StoredFood += organism.CurrentEnergy * energyGainEfficiency;
                organism.Remove();
                return organism.CurrentEnergy;
            }
            else
            {
                organism.CurrentEnergy -= foodToBeEaten;
                StoredFood += foodToBeEaten * energyGainEfficiency;
                return foodToBeEaten;
            }
        }

        private void EatPlant(Plant plant)
        {
            double amountEaten = Eat(plant, new Percentage(100 - Properties.CarnivorityTendency.Value));
            double poisonEaten = plant.Properties.Toxicity.Value * amountEaten / 3d;

            Injuries += Math.Max(0, poisonEaten - Properties.PlantToxicityResistance.Value);
        }

        public override void Update()
        {
            Injuries--;
            BasicUpdate(new Percentage(Properties.GrowthRate.Value), new Percentage(Properties.BackupEnergy.Value), new Percentage(Properties.ReproductionEnergy.Value));
        }

        private List<Organism> GetOrganismsInSight()
        {
            return GetSimulationObjectsInRange(Properties.Eyesight.Value).Where(obj => obj is Organism).Select(obj => (Organism)obj).ToList();
        }

        private List<Animal> GetAnimalsInSight(List<Organism> organisms)
        {
            return organisms.Where(obj => obj is Animal).Select(obj => (Animal)obj).ToList();
        }

        private List<Plant> GetPlantsInSight(List<Organism> organisms)
        {
            return organisms.Where(obj => obj is Plant).Select(obj => (Plant)obj).ToList();
        }

        private bool IsAThreat(Animal animal) 
        {
            double thisThreatValue = CurrentSize * (Properties.OffensiveCapability.Value + Properties.DefensiveCapability.Value);
            double animalThreatValue = animal.CurrentSize * (animal.Properties.OffensiveCapability.Value + animal.Properties.DefensiveCapability.Value);

            return animalThreatValue - thisThreatValue > Properties.ThreatConsiderationLimit.Value;
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

            List<Animal> newOffspring = new List<Animal>();

            for (int i = 0; i < offspringAmount; i++)
            {
                Animal offspring = new Animal(Position, energyForOneOffspring, Properties.CreateMutated());
                Relatives.Add(offspring);
                offspring.Relatives.Add(this);
                Scene.Add(offspring);
                newOffspring.Add(offspring);
            }

            foreach(Animal os in newOffspring)
            {
                List<Animal> withoutItself = new List<Animal>(newOffspring);
                withoutItself.Remove(os);
                os.Relatives.AddRange(withoutItself);
            }
        }
    }
}
