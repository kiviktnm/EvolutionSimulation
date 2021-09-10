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

            // This rather unoptimized way is used to maximize the stored energy offspring have and minimize their size
            CurrentSize = 1;
            startingEnergy--;

            while(startingEnergy > 0)
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

        private Environment currentEnv;
        public Environment Environment
        {
            get
            {
                Environment env = Manager.GetEnvironment(Position);
                if (env != null)
                    currentEnv = env;

                return currentEnv;
            }
        }
        public AnimalProperties Properties { get; }
        public override double MaxSize => Properties.AdultSize.Value;

        private List<Animal> Relatives = new List<Animal>();
        public AnimalObjective CurrentObjective { get; private set; } = AnimalObjective.FindFood;
        private Animal currentTarget;
        private Plant currentPlantTarget;
        private bool defensiveFight = false;
        private Point randomMovementPoint = new Point(int.MaxValue, int.MinValue);

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
                + Properties.FoodStoringAndDigestingCapability.Value * CurrentSize / 100d
                + Properties.EnvironmentToxicityResistance.Value / 2
                + Properties.TemperatureChangeResistance.Value / 2 * (CurrentSize / 2d)
                + Math.Max(0, Environment.Toxicity.Value - Properties.EnvironmentToxicityResistance.Value)
                + Math.Abs(Properties.OptimalTemperature.Value - Environment.Temperature.Value) / Properties.TemperatureChangeResistance.Value * (CurrentSize / 2d)
                + Injuries;
        }

        private double FoodStoringCapacity => Properties.FoodStoringAndDigestingCapability.Value * EnergyStoringCapacity;
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

                double amountDigested = EnergyConsumption + Properties.FoodStoringAndDigestingCapability.Value;
                if (amountDigested > StoredFood) 
                {
                    return StoredFood;
                }

                return amountDigested;
            }
        }

        // Returns the amount eaten from an organism
        private double Eat(Organism organism, Percentage energyGainEfficiency)
        {
            double foodToBeEaten = (FoodStoringCapacity - StoredFood) / energyGainEfficiency;

            if (organism.CurrentEnergy + organism.CurrentSize * 0.5d < foodToBeEaten) 
            {
                StoredFood += (organism.CurrentEnergy + organism.CurrentSize * 0.5d) * energyGainEfficiency;
                organism.Remove();
                return organism.CurrentEnergy + organism.CurrentSize * 0.5d;
            }
            else
            {
                organism.CurrentEnergy -= foodToBeEaten;

                // Since the organism would die anyways due to losing energy below zero and we know that organism.CurrentEnergy + organism.CurrentSize * 0.5d > foodToBeEaten
                // then nothing needs to be done here

                StoredFood += foodToBeEaten * energyGainEfficiency;
                return foodToBeEaten;
            }
        }

        public override void Update()
        {
            Injuries--;
            BasicUpdate(new Percentage(Properties.GrowthRate.Value), new Percentage(Properties.BackupEnergy.Value), new Percentage(Properties.ReproductionEnergy.Value));
            StoredFood -= EnergyProduction;

            if (CurrentObjective == AnimalObjective.Fight)
            {
                Fight();
                return;
            }

            List<Organism> visible = GetOrganismsInSight();
            List<Animal> animals = GetAnimalsInSight(visible);
            List<Plant> plants = GetPlantsInSight(visible);

            CheckForThreats(animals);

            if (CurrentObjective == AnimalObjective.RunAway)
            {
                if (animals.Contains(currentTarget))
                {
                    Escape();
                    return;
                }
                CurrentObjective = AnimalObjective.FindFood;
            }

            // This is done here because the CurrentObjective may change during LookForFood
            if (CurrentObjective == AnimalObjective.FindFood)
            {
                LookForFood(animals, plants);
            }

            switch (CurrentObjective)
            {
                case AnimalObjective.FindFood:
                    MoveRandomly();
                    break;
                case AnimalObjective.EatPlant:
                    EatPlant();
                    break;
                case AnimalObjective.EatAnimal:
                    EatAnimal();
                    break;
            }
        }

        private void Fight()
        {
            currentTarget.Injuries += Math.Max(CurrentSize * Properties.OffensiveCapability.Value / 2d - currentTarget.CurrentSize * currentTarget.Properties.DefensiveCapability.Value, 0);

            if (currentTarget.IsRemoved)
            {
                if (!defensiveFight)
                    Eat(currentTarget, new Percentage(Properties.CarnivorityTendency.Value));

                CurrentObjective = AnimalObjective.FindFood;
            }
        }

        private void EatAnimal()
        {
            if (Position.DistanceToSqr(currentTarget.Position) <= CurrentSize / 2d + currentTarget.CurrentSize / 2d)
            {
                defensiveFight = false;
                CurrentObjective = AnimalObjective.Fight;
                currentTarget.CurrentObjective = AnimalObjective.Fight;
                currentTarget.currentTarget = this;
                currentTarget.defensiveFight = true;
                Fight();
            }
            else
            {
                Move(currentPlantTarget.Position);
            }
        }

        private void EatPlant() 
        {
            if (Position.DistanceToSqr(currentPlantTarget.Position) == 0) 
            {
                double amountEaten = Eat(currentPlantTarget, new Percentage(100 - Properties.CarnivorityTendency.Value));
                double poisonEaten = currentPlantTarget.Properties.Toxicity.Value * amountEaten;

                Injuries += Math.Max(0, poisonEaten - Properties.PlantToxicityResistance.Value);

                CurrentObjective = AnimalObjective.FindFood;
            }
            else 
            {
                Move(currentPlantTarget.Position);
            }
        }

        private void MoveRandomly()
        {
            if (randomMovementPoint.Equals(new Point(int.MaxValue,int.MinValue)) || randomMovementPoint.DistanceTo(Position) == 0)
            {
                double posX = Random.Double(Position.X - Properties.Eyesight.Value, Position.X + Properties.Eyesight.Value);
                double posY = Random.Double(Position.Y - Properties.Eyesight.Value, Position.Y + Properties.Eyesight.Value);
                randomMovementPoint = new Point(posX, posY);
            }
            
            Move(randomMovementPoint);
        }

        private void Escape()
        {
            Point targetPosition = new Point(2 * Position.X - currentTarget.Position.X, 2 * Position.Y - currentTarget.Position.Y);
            Move(targetPosition);
        }

        private void Move(Point towards)
        {
            Point oldPos = Position;
            MoveTowards(towards, Properties.MovementSpeed.Value);

            double dist = oldPos.DistanceTo(Position);
            CurrentEnergy -= dist * 0.1;
        }

        private void LookForFood(List<Animal> animals, List<Plant> plants) 
        {
            bool prefersAnimals = Properties.CarnivorityTendency.Value > 50;
            bool pureCarnivore = Properties.CarnivorityTendency.Value > 75;
            bool pureHerbivore = Properties.CarnivorityTendency.Value < 25;

            Animal animalFoodCanditate = LookForAnimalFood(animals);
            Plant plantFoodCanditate = LookForPlantFood(plants);

            if (animalFoodCanditate == null && plantFoodCanditate == null)
                return;

            if (animalFoodCanditate == null && !pureCarnivore)
            {
                currentPlantTarget = plantFoodCanditate;
                CurrentObjective = AnimalObjective.EatPlant;
                return;
            }

            if (plantFoodCanditate == null && !pureHerbivore)
            {
                currentTarget = animalFoodCanditate;
                CurrentObjective = AnimalObjective.EatAnimal;
                return;
            }

            if (prefersAnimals)
            {
                currentTarget = animalFoodCanditate;
                CurrentObjective = AnimalObjective.EatAnimal;
            }
            else
            {
                currentPlantTarget = plantFoodCanditate;
                CurrentObjective = AnimalObjective.EatPlant;
            }
        }

        private Animal LookForAnimalFood(List<Animal> animals)
        {
            Animal lowestThreat = null;
            double lowestThreatVal = 0;

            foreach (Animal animal in animals)
            {
                // Do not eat your relatives!
                if (Relatives.Contains(animal)) continue;
                // Also don't eat yourself!
                if (animal == this) continue;

                double thrVal = GetThreatValue(animal);
                if (thrVal < lowestThreatVal)
                {
                    lowestThreat = animal;
                    lowestThreatVal = thrVal;
                }
            }

            return lowestThreat;
        }

        private Plant LookForPlantFood(List<Plant> plants)
        {
            /*                 HUOM!
             * ---------------------------------------
             * Should animals be able to see plant toxicity?
             */
            Plant closest = null;
            foreach (Plant plant in plants) 
            {
                if (closest == null) 
                {
                    closest = plant;
                }

                if (plant.Position.DistanceToSqr(Position) < closest.Position.DistanceToSqr(Position))
                {
                    closest = plant;
                }
            }

            return closest;
        }

        private void CheckForThreats(List<Animal> animals)
        {
            Animal highestThreat = null;
            double highestThreatVal = 0;

            foreach (Animal animal in animals)
            {
                // Relatives are no threats
                if (Relatives.Contains(animal)) continue;
                // OFC you aren't a threat to yourself right?
                if (animal == this) continue;

                double thrVal = GetThreatValue(animal);
                if (thrVal > highestThreatVal)
                {
                    highestThreat = animal;
                    highestThreatVal = thrVal;
                }
            }

            if (highestThreat != null)
            {
                currentTarget = highestThreat;
                CurrentObjective = AnimalObjective.RunAway;
            }
        }

        private List<Organism> GetOrganismsInSight()
        {
            return GetSimulationObjectsInRange(Properties.Eyesight.Value + CurrentSize / 2d).Where(obj => obj is Organism).Select(obj => (Organism)obj).ToList();
        }

        private List<Animal> GetAnimalsInSight(List<Organism> organisms)
        {
            return organisms.Where(obj => obj is Animal).Select(obj => (Animal)obj).ToList();
        }

        private List<Plant> GetPlantsInSight(List<Organism> organisms)
        {
            return organisms.Where(obj => obj is Plant).Select(obj => (Plant)obj).ToList();
        }

        // Returns a number greater than 0 if an animal is a threat. A larger number indicates a bigger threat
        private double GetThreatValue(Animal animal) 
        {
            double thisThreatValue = CurrentSize * (Properties.OffensiveCapability.Value + Properties.DefensiveCapability.Value);
            double animalThreatValue = animal.CurrentSize * (animal.Properties.OffensiveCapability.Value + animal.Properties.DefensiveCapability.Value);

            return (animalThreatValue - thisThreatValue) - Properties.ThreatConsiderationLimit.Value;
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
