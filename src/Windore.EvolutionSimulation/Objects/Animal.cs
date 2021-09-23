using System;
using System.Collections.Generic;
using System.Linq;
using Windore.Simulations2D.Util;
using Windore.Simulations2D.Util.SMath;

namespace Windore.EvolutionSimulation.Objects
{
    public class Animal : Organism
    {
        public Animal(Point position, double startingEnergy, double startingSize, AnimalProperties properties) : base(new Shape(position, 1, 1, false),
                  new Color(
                      (byte)(255 - (155 * properties.CarnivorityTendency.Value / properties.CarnivorityTendency.MaxValue)),
                      0,
                      (byte)((255 * properties.CarnivorityTendency.Value / properties.CarnivorityTendency.MaxValue) - 100))
                  )
        {
            Properties = properties;
            CurrentSize = startingSize;
            CurrentEnergy = startingEnergy / 2d;
            StoredFood = startingEnergy / 2d;
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
        private Point randomMovementPoint = noneRandomMovePoint; 
        private static readonly Point noneRandomMovePoint = new Point(int.MaxValue, int.MinValue);

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
            get => SimulationSettings.ENERGY_COEFFICIENT * (0.5d * CurrentSize
                + Properties.MovementSpeed.Value / 2d
                + Properties.Eyesight.Value / 8d
                + Properties.OffensiveCapability.Value * CurrentSize / 3d
                + Properties.DefensiveCapability.Value * CurrentSize / 3d
                + Properties.PlantToxicityResistance.Value / 2d * CurrentSize
                + Properties.FoodDigestingCapability.Value * CurrentSize / 100d
                + Properties.EnvironmentToxicityResistance.Value / 2
                + Properties.TemperatureChangeResistance.Value / 2 * (CurrentSize / 2d)
                + Math.Max(0, Environment.Toxicity.Value - Properties.EnvironmentToxicityResistance.Value) / 10d
                + Math.Abs(Properties.OptimalTemperature.Value - Environment.Temperature.Value) / Properties.TemperatureChangeResistance.Value * (CurrentSize / 2d)
                + Injuries);
        }

        private double FoodStoringCapacity => EnergyStoringCapacity;
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

                // progressive food digesting
                Percentage foodStoredPercent = Percentage.FromDouble(1d + StoredFood / FoodStoringCapacity); 
                double extraAmountDigested = StoredFood * new Percentage(Properties.FoodDigestingCapability.Value) * foodStoredPercent;

                double amountDigested = EnergyConsumption + extraAmountDigested;
                if (amountDigested > StoredFood) 
                {
                    return StoredFood;
                }

                return amountDigested;
            }
        }

        private int eggState = 10;

        // Returns the amount eaten from an organism
        private double Eat(Organism organism, Percentage energyGainEfficiency)
        {
            double foodToBeEaten = (FoodStoringCapacity - StoredFood) / energyGainEfficiency;

            if (organism.CurrentEnergy + organism.CurrentSize < foodToBeEaten) 
            {
                StoredFood += (organism.CurrentEnergy + organism.CurrentSize) * energyGainEfficiency;
                organism.Remove();
                return organism.CurrentEnergy + organism.CurrentSize;
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
            if (eggState-- > 0)
            {
                return;
            }

            Injuries--;
            BasicUpdate(new Percentage(Properties.BackupEnergy.Value), new Percentage(Properties.ReproductionEnergy.Value));
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
                    randomMovementPoint = noneRandomMovePoint;
                    return;
                }
                CurrentObjective = AnimalObjective.FindFood;
            }

            if (CurrentObjective == AnimalObjective.EatPlant && currentPlantTarget.IsRemoved) CurrentObjective = AnimalObjective.FindFood;
            if (CurrentObjective == AnimalObjective.EatAnimal && currentTarget.IsRemoved) CurrentObjective = AnimalObjective.FindFood;

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

            if (CurrentObjective != AnimalObjective.FindFood)
            {
                randomMovementPoint = noneRandomMovePoint;
            }
        }

        private void Fight()
        {
            currentTarget.Injuries += Math.Max((3 * CurrentSize * Properties.OffensiveCapability.Value) / (2 * currentTarget.CurrentSize * currentTarget.Properties.DefensiveCapability.Value), 0.1);

            if (currentTarget.IsRemoved)
            {
                if (!defensiveFight)
                    Eat(currentTarget, new Percentage(Properties.CarnivorityTendency.Value));

                CurrentObjective = AnimalObjective.FindFood;
            }
        }

        private void EatAnimal()
        {
            if (currentTarget.IsRemoved)
            {
                CurrentObjective = AnimalObjective.FindFood;
            }

            Move(currentTarget.Position);

            if (Position.DistanceToSqr(currentTarget.Position) <= CurrentSize / 2d + currentTarget.CurrentSize / 2d)
            {
                defensiveFight = false;
                CurrentObjective = AnimalObjective.Fight;
                currentTarget.CurrentObjective = AnimalObjective.Fight;
                currentTarget.currentTarget = this;
                currentTarget.defensiveFight = true;
                Fight();
            }
        }

        private void EatPlant() 
        {
            if (currentPlantTarget.IsRemoved)
            {
                CurrentObjective = AnimalObjective.FindFood;
            }

            Move(currentPlantTarget.Position);

            if (Position.DistanceToSqr(currentPlantTarget.Position) == 0) 
            {
                double amountEaten = Eat(currentPlantTarget, new Percentage(100 - Properties.CarnivorityTendency.Value));
                double poisonEaten = currentPlantTarget.Properties.Toxicity.Value * amountEaten;

                Injuries += Math.Max(0, poisonEaten - (Properties.PlantToxicityResistance.Value * CurrentSize));

                CurrentObjective = AnimalObjective.FindFood;
            }
        }

        private void MoveRandomly()
        {
            if (randomMovementPoint.Equals(noneRandomMovePoint) || randomMovementPoint.DistanceTo(Position) == 0)
            {
                double posX = Random.Double(Position.X - 2d * Properties.Eyesight.Value, Position.X + 2d * Properties.Eyesight.Value);
                double posY = Random.Double(Position.Y - 2d * Properties.Eyesight.Value, Position.Y + 2d * Properties.Eyesight.Value);
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
            CurrentEnergy -= dist * SimulationSettings.ENERGY_COEFFICIENT;
        }

        private void LookForFood(List<Animal> animals, List<Plant> plants) 
        {
            bool pureCarnivore = Properties.CarnivorityTendency.Value > 75;
            bool pureHerbivore = Properties.CarnivorityTendency.Value < 25;

            Animal animalFoodCanditate = LookForAnimalFood(animals);
            Plant plantFoodCanditate = LookForPlantFood(plants);

            if (animalFoodCanditate == null && plantFoodCanditate == null)
                return;

            if (pureCarnivore && animalFoodCanditate == null) return;
            if (pureHerbivore && plantFoodCanditate == null) return;

            if (animalFoodCanditate == null)
            {
                currentPlantTarget = plantFoodCanditate;
                CurrentObjective = AnimalObjective.EatPlant;
                return;
            }

            if (plantFoodCanditate == null)
            {
                currentTarget = animalFoodCanditate;
                CurrentObjective = AnimalObjective.EatAnimal;
                return;
            }

            if (SimulationSettings.Instance.SimulationRandom.Boolean(new Percentage(Properties.CarnivorityTendency.Value)))
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
            /*  
             * Animals look for the plant which would give them the most energy. 
             */
            Plant closest = null;
            double energyNum = 0;
            foreach (Plant plant in plants) 
            {
                double plantEn = plant.CurrentSize - plant.Position.DistanceToSqr(Position) * 0.1d;
                if (closest == null) 
                {
                    closest = plant;
                    energyNum = plantEn;
                }

                if (plantEn > energyNum)
                {
                    closest = plant;
                    energyNum = plantEn;
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
            return GetSimulationObjectsInRange(Properties.Eyesight.Value + CurrentSize).Where(obj => obj is Organism).Select(obj => (Organism)obj).ToList();
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
            double reproEnergy = new Percentage(Properties.ReproductionEnergy.Value) * EnergyStoringCapacity;
            CurrentEnergy -= reproEnergy;

            List<Animal> newOffspring = new List<Animal>();

            int actualOffspringAmount = 0;
            int integerOffspringAmount = (int)Math.Floor(Properties.OffspringAmount.Value);
            
            Percentage percentageForAdditionalOffspring = Percentage.FromDouble(Properties.OffspringAmount.Value - integerOffspringAmount);
            if (SimulationSettings.Instance.SimulationRandom.Boolean(percentageForAdditionalOffspring))
                actualOffspringAmount++;

            actualOffspringAmount += integerOffspringAmount;


            double energyForOffspring = reproEnergy / actualOffspringAmount;

            while (reproEnergy - energyForOffspring >= 0) 
            {
                Animal offspring = new Animal(Position, energyForOffspring * 4d/5d, energyForOffspring * 1d/5d, Properties.CreateMutated());
                offspring.currentEnv = currentEnv;
                offspring.Generation = Generation + 1;

                Relatives.Add(offspring);
                offspring.Relatives.Add(this);
                Scene.Add(offspring);
                newOffspring.Add(offspring);
                reproEnergy -= energyForOffspring;
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
