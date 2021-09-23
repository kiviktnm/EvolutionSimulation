using System;
using System.Collections.Generic;
using System.IO;
using Windore.EvolutionSimulation.Objects;
using Windore.Settings.Base;
using Windore.Settings.GUI;
using Windore.Simulations2D;
using Windore.Simulations2D.Util;
using Windore.Simulations2D.Util.SMath;

namespace Windore.EvolutionSimulation
{
    public class SimulationSettings
    {
        public const double ENERGY_COEFFICIENT = 0.04d; // 0,05 hyvä luku

        [Setting("Simulation Area Side Length", "General")]
        [DoubleSettingValueLimits(100, 2000)]
        public double SimulationSceneSideLength { get; set; } = 1_500;

        [Setting("Mutation Probability", "General")]
        [DoubleSettingValueLimits(0, 100)]
        public double MutationProbability { get; set; } = 10;

        [Setting("Randomness Seed (0 for random seed)", "General")]
        public int RandomnessSeed { get => SimulationRandom.Seed; set => SimulationRandom.Seed = value; }

        [Setting("Simulation Log Directory", "General")]
        [StringSettingIsPath]
        public string SimulationLogDirectory { get; set; } = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "EvolutionSimulation/");


        #region Plant Starting Properties

        private PlantProperties startingPlantProperties = new PlantProperties
        {
            AdultSize = new Property(20, 1, 1000, 50),
            MutationStrength = new Property(1, 0.5, 40, 5),
            ReproductionEnergy = new Property(50, 0, 100, 50),
            OffspringAmount = new Property(2, 1, 10, 3),
            BackupEnergy = new Property(20, 0, 100, 20),
            OptimalTemperature = new Property(30, 0, 80, 50),
            TemperatureChangeResistance = new Property(5, 1, 100, 10),
            EnvironmentToxicityResistance = new Property(0, 0, 100, 10),
            Toxicity = new Property(0, 0, 100, 10),
            ReproductionArea = new Property(40, 1, 300, 50),
            EnergyProductionInLowNutrientGrounds = new Property(0, 0, 20, 5)
        };

        [Setting("Adult Size", "Plant Starting Properties")]
        [DoubleSettingValueLimits(1, 1000)]
        public double AdultSizePP { get => startingPlantProperties.AdultSize.Value; set => startingPlantProperties.AdultSize.Value = value; }

        [Setting("Mutation Strength", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0.5, 40)]
        public double MutationStrengthPP { get => startingPlantProperties.MutationStrength.Value; set => startingPlantProperties.MutationStrength.Value = value; }

        [Setting("Reproduction Energy", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double ReproductionEnergyPP { get => startingPlantProperties.ReproductionEnergy.Value; set => startingPlantProperties.ReproductionEnergy.Value = value; }

        [Setting("Offspring Amount", "Offspring Amount")]
        [DoubleSettingValueLimits(0, 10)]
        public double OffspringAmountPP { get => startingPlantProperties.OffspringAmount.Value; set => startingPlantProperties.OffspringAmount.Value = value; }


        [Setting("Backup Energy", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double BackupEnergyPP { get => startingPlantProperties.BackupEnergy.Value; set => startingPlantProperties.BackupEnergy.Value = value; }

        [Setting("Optimal Temperature", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 80)]
        public double OptimalTemperaturePP { get => startingPlantProperties.OptimalTemperature.Value; set => startingPlantProperties.OptimalTemperature.Value = value; }

        [Setting("Temperature Change Resistance", "Plant Starting Properties")]
        [DoubleSettingValueLimits(1, 100)]
        public double TemperatureChangeResistancePP { get => startingPlantProperties.TemperatureChangeResistance.Value; set => startingPlantProperties.TemperatureChangeResistance.Value = value; }

        [Setting("Environment Toxicity Resistance", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double EnvironmentToxicityResistancePP { get => startingPlantProperties.EnvironmentToxicityResistance.Value; set => startingPlantProperties.EnvironmentToxicityResistance.Value = value; }

        [Setting("Toxicity", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double ToxicityPP { get => startingPlantProperties.Toxicity.Value; set => startingPlantProperties.Toxicity.Value = value; }

        [Setting("Reproduction Area", "Plant Starting Properties")]
        [DoubleSettingValueLimits(1, 300)]
        public double ReproductionAreaPP { get => startingPlantProperties.ReproductionArea.Value; set => startingPlantProperties.ReproductionArea.Value = value; }

        [Setting("Energy Production In Low Nutrient Grounds", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 20)]
        public double EnergyProductionInLowNutrientGroundsPP { get => startingPlantProperties.EnergyProductionInLowNutrientGrounds.Value; set => startingPlantProperties.EnergyProductionInLowNutrientGrounds.Value = value; }

        #endregion

        #region Animal Starting Properties

        private AnimalProperties startingAnimalProperties = new AnimalProperties
        {
            AdultSize = new Property(20, 1, 1000, 50),
            MutationStrength = new Property(1, 0.5, 40, 5),
            ReproductionEnergy = new Property(50, 0, 100, 50),
            OffspringAmount = new Property(2, 1, 10, 3),
            BackupEnergy = new Property(20, 0, 100, 20),
            OptimalTemperature = new Property(30, 0, 80, 50),
            TemperatureChangeResistance = new Property(5, 1, 100, 10),
            EnvironmentToxicityResistance = new Property(0, 0, 100, 10),
            MovementSpeed = new Property(10, 0, 40, 10),
            CarnivorityTendency = new Property(50, 0, 100, 50),
            Eyesight = new Property(20, 0, 100, 20),
            OffensiveCapability = new Property(2, 0, 20, 8),
            DefensiveCapability = new Property(2, 0, 20, 8),
            ThreatConsiderationLimit = new Property(10, 0, 40, 15),
            PlantToxicityResistance = new Property(0, 0, 100, 30),
            FoodDigestingCapability = new Property(10, 1, 100, 10)
        };

        [Setting("Adult Size", "Animal Starting Properties")]
        [DoubleSettingValueLimits(1, 1000)]
        public double AdultSizeAP { get => startingAnimalProperties.AdultSize.Value; set => startingAnimalProperties.AdultSize.Value = value; }

        [Setting("Mutation Strength", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0.5, 40)]
        public double MutationStrengthAP { get => startingAnimalProperties.MutationStrength.Value; set => startingAnimalProperties.MutationStrength.Value = value; }

        [Setting("Offspring Amount", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 10)]
        public double OffspringAmountAP { get => startingAnimalProperties.OffspringAmount.Value; set => startingAnimalProperties.OffspringAmount.Value = value; }

        [Setting("Reproduction Energy", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double ReproductionEnergyAP { get => startingAnimalProperties.ReproductionEnergy.Value; set => startingAnimalProperties.ReproductionEnergy.Value = value; }

        [Setting("Backup Energy", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double BackupEnergyAP { get => startingAnimalProperties.BackupEnergy.Value; set => startingAnimalProperties.BackupEnergy.Value = value; }

        [Setting("Optimal Temperature", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 80)]
        public double OptimalTemperatureAP { get => startingAnimalProperties.OptimalTemperature.Value; set => startingAnimalProperties.OptimalTemperature.Value = value; }

        [Setting("Temperature Change Resistance", "Animal Starting Properties")]
        [DoubleSettingValueLimits(1, 100)]
        public double TemperatureChangeResistanceAP { get => startingAnimalProperties.TemperatureChangeResistance.Value; set => startingAnimalProperties.TemperatureChangeResistance.Value = value; }

        [Setting("Environment Toxicity Resistance", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double EnvironmentToxicityResistanceAP { get => startingAnimalProperties.EnvironmentToxicityResistance.Value; set => startingAnimalProperties.EnvironmentToxicityResistance.Value = value; }

        [Setting("Movement Speed", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 40)]
        public double MovementSpeedAP { get => startingAnimalProperties.MovementSpeed.Value; set => startingAnimalProperties.MovementSpeed.Value = value; }

        [Setting("Carnivority Tendency", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double CarnivorityTendencyAP { get => startingAnimalProperties.CarnivorityTendency.Value; set => startingAnimalProperties.CarnivorityTendency.Value = value; }

        [Setting("Eyesight", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double EyesightAP { get => startingAnimalProperties.Eyesight.Value; set => startingAnimalProperties.Eyesight.Value = value; }

        [Setting("Offensive Capability", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 20)]
        public double OffensiveCapabilityAP { get => startingAnimalProperties.OffensiveCapability.Value; set => startingAnimalProperties.OffensiveCapability.Value = value; }

        [Setting("Defensive Capability", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 20)]
        public double DefensiveCapabilityAP { get => startingAnimalProperties.DefensiveCapability.Value; set => startingAnimalProperties.DefensiveCapability.Value = value; }

        [Setting("Threat Consideration Limit", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 40)]
        public double ThreatConsiderationLimitAP { get => startingAnimalProperties.ThreatConsiderationLimit.Value; set => startingAnimalProperties.ThreatConsiderationLimit.Value = value; }

        [Setting("Plant Toxicity Resistance", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double PlantToxicityResistanceAP { get => startingAnimalProperties.PlantToxicityResistance.Value; set => startingAnimalProperties.PlantToxicityResistance.Value = value; }

        [Setting("Food Digesting Capability", "Animal Starting Properties")]
        [DoubleSettingValueLimits(1, 100)]
        public double FoodDigestingCapabilityAP { get => startingAnimalProperties.FoodDigestingCapability.Value; set => startingAnimalProperties.FoodDigestingCapability.Value = value; }

        #endregion

        private static SimulationSettings ins;
        public static SimulationSettings Instance 
        {
            get 
            {
                if (ins == null) 
                {
                    ins = new SimulationSettings();
                }
                return ins;
            }
        }

        public SRandom SimulationRandom { get; private set; } = new SRandom(0);
        private Manager smng;
        public Manager SimulationManager
        {
            get
            {
                if (smng == null) 
                {
                    SimulationScene scene = new SimulationScene(Instance.SimulationSceneSideLength, Instance.SimulationSceneSideLength);
                    smng = new Manager(scene);

                    smng.SetUpEnvs();

                    Point startingPoint = new Point(scene.Width * 0.50, scene.Height * 0.66);

                    for (int i = 0; i < 150; i++)
                    {
                        // Bruteforce a position within an environment
                        Point pos;
                        do
                        {
                            double range = SimulationSceneSideLength * 0.2075d;
                            double posX = SimulationRandom.Double(startingPoint.X - range, startingPoint.X + range);
                            double posY = SimulationRandom.Double(startingPoint.Y - range, startingPoint.Y + range);

                            pos = new Point(posX, posY);
                        } while (SimulationManager.GetEnvironment(pos) == null);


                        double startingEnergy = SimulationRandom.Double(2, 40);
                        Plant startingPlant = new Plant(pos, startingEnergy, startingEnergy / 2d, startingPlantProperties);

                        scene.Add(startingPlant);
                    }
                    Plant startingCenterPlant = new Plant(startingPoint, 20, 10, startingPlantProperties);
                    scene.Add(startingCenterPlant);

                    // This animal will die rather immediatly, but adding it here is necessary since logging requires it to exist
                    AddStartingAnimal();
                    // The "real" starting animal is added later to the simulation

                    // Logging must be set up only after all object types that will be logged are added to the simulation.
                    smng.SetUpLogging();
                }
                return smng;
            }
        }

        public void AddStartingAnimal()
        {
            Point startingPoint = new Point(smng.SimulationScene.Width * 0.50, smng.SimulationScene.Height * 0.66);
            Animal startingAnimal = new Animal(startingPoint, 20, 10, startingAnimalProperties)
            {
                StoredFood = 10
            };
            smng.SimulationScene.Add(startingAnimal);
        }

        private SettingsManager<SimulationSettings> manager = new SettingsManager<SimulationSettings>();

        private SimulationSettings() 
        {
            manager.SetSettingObject(this);
        }

        public void InitSimulation() 
        {
            InitSRandom();
            CloseSettingsWindow();
            WriteSettingsFile();
        }

        private void WriteSettingsFile()
        {
            Directory.CreateDirectory(SimulationLogDirectory);
            string text = manager.GenerateSettingsString();

            string fileName = Path.Combine(SimulationLogDirectory, "simulation-settings-file");

            // Find a file that doesn't exist by adding numbers in the end of the file name
            int num = 0;
            string addition = $"-0.txt";

            while (File.Exists(fileName + addition))
            {
                num++;
                addition = $"-{num}.txt";
            }
            fileName += addition;

            File.WriteAllText(fileName, text);
        }

        private void InitSRandom()
        {
            if (RandomnessSeed == 0)
            {
                SimulationRandom = new SRandom();
            }
        }

        private SettingsWindow window;

        public void OpenSettingsWindow() 
        {
            if (window != null)
                window.Close();

            window = new SettingsWindow(manager);
            window.Show();
        }

        private void CloseSettingsWindow()
        {
            if (window == null)
                return;

            window.Close();
        }
    }
}