using System.IO;
using Windore.Settings.Base;
using Windore.EvolutionSimulation.Objects;

namespace Windore.EvolutionSimulation
{
    public class SimulationSettings
    {
        #region General Settings

        [Setting("Start Paused", "General")]
        public bool StartPaused { get; set; } = false;

        [Setting("Simulation Area Side Length", "General")]
        [DoubleSettingValueLimits(100, 2000)]
        public double SimulationSceneSideLength { get; set; } = 1_500;

        [Setting("Mutation Probability", "General")]
        [DoubleSettingValueLimits(0, 100)]
        public double MutationProbability { get; set; } = 10;

        [Setting("Randomness Seed (0 for random seed)", "General")]
        public int RandomnessSeed { get; set; } = 0;

        [Setting("Simulation Log Directory", "General")]
        [StringSettingIsPath]
        public string SimulationLogDirectory { get; set; } = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "EvolutionSimulation/");

        #endregion

        #region Environment Properties

        #region Base Env

        [Setting("Temperature", "Base Environment Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double BaseEnvTemperature { get; set; } = 30;
        [Setting("Toxicity", "Base Environment Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double BaseEnvToxicity { get; set; } = 10;
        [Setting("Ground Nutrient Content", "Base Environment Properties")]
        [DoubleSettingValueLimits(0, 20)]
        public double BaseEnvGroundNutrientContent { get; set; } = 10;

        [Setting("Temperature Change Per Update", "Base Environment Properties")]
        [DoubleSettingValueLimits(-1, 1)]
        public double BaseEnvTemperatureCPU { get; set; } = 0;
        [Setting("Toxicity Change Per Update", "Base Environment Properties")]
        [DoubleSettingValueLimits(-1, 1)]
        public double BaseEnvToxicityCPU { get; set; } = 0;
        [Setting("Ground Nutrient Content Change Per Update", "Base Environment Properties")]
        [DoubleSettingValueLimits(-1, 1)]
        public double BaseEnvGroundNutrientContentCPU { get; set; } = 0;

        [Setting("Reverse Value Changing", "Base Environment Properties")]
        public bool BaseEnvReverseChanging { get; set; } = false;

        #endregion

        #region Center Env

        [Setting("Temperature Difference From Base", "Center Environment Properties")]
        [DoubleSettingValueLimits(-100, 100)]
        public double CEnvTemperature { get; set; } = 0;
        [Setting("Toxicity Difference From Base", "Center Environment Properties")]
        [DoubleSettingValueLimits(-100, 100)]
        public double CEnvToxicity { get; set; } = 0;
        [Setting("Ground Nutrient Content Difference From Base", "Center Environment Properties")]
        [DoubleSettingValueLimits(-20, 20)]
        public double CEnvGroundNutrientContent { get; set; } = 0;

        [Setting("Temperature Max Difference From Base", "Center Environment Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double CEnvTemperatureMaxDiff { get; set; } = 0;
        [Setting("Toxicity Max Difference From Base", "Center Environment Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double CEnvToxicityMaxDiff { get; set; } = 0;
        [Setting("Ground Nutrient Content Max Difference From Base", "Center Environment Properties")]
        [DoubleSettingValueLimits(0, 20)]
        public double CEnvGroundNutrientContentMaxDiff { get; set; } = 10;

        [Setting("Temperature Difference Change Per Update", "Center Environment Properties")]
        [DoubleSettingValueLimits(-1, 1)]
        public double CEnvTemperatureCPU { get; set; } = 0;
        [Setting("Toxicity Difference Change Per Update", "Center Environment Properties")]
        [DoubleSettingValueLimits(-1, 1)]
        public double CEnvToxicityCPU { get; set; } = 0;
        [Setting("Ground Nutrient Content Difference Change Per Update", "Center Environment Properties")]
        [DoubleSettingValueLimits(-1, 1)]
        public double CEnvGroundNutrientContentCPU { get; set; } = 1d/600d;

        [Setting("Reverse Difference Value Changing", "Center Environment Properties")]
        public bool CEnvReverseChanging { get; set; } = true;

        #endregion
    
        #region Side 1 Env

        [Setting("Temperature Difference From Base", "Side Environment 1 Properties")]
        [DoubleSettingValueLimits(-100, 100)]
        public double S1EnvTemperature { get; set; } = 5;
        [Setting("Toxicity Difference From Base", "Side Environment 1 Properties")]
        [DoubleSettingValueLimits(-100, 100)]
        public double S1EnvToxicity { get; set; } = 0;
        [Setting("Ground Nutrient Content Difference From Base", "Side Environment 1 Properties")]
        [DoubleSettingValueLimits(-20, 20)]
        public double S1EnvGroundNutrientContent { get; set; } = 0;

        [Setting("Temperature Max Difference From Base", "Side Environment 1 Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double S1EnvTemperatureMaxDiff { get; set; } = 10;
        [Setting("Toxicity Max Difference From Base", "Side Environment 1 Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double S1EnvToxicityMaxDiff { get; set; } = 0;
        [Setting("Ground Nutrient Content Max Difference From Base", "Side Environment 1 Properties")]
        [DoubleSettingValueLimits(0, 20)]
        public double S1EnvGroundNutrientContentMaxDiff { get; set; } = 0;

        [Setting("Temperature Difference Change Per Update", "Side Environment 1 Properties")]
        [DoubleSettingValueLimits(-1, 1)]
        public double S1EnvTemperatureCPU { get; set; } = -1d/600d;
        [Setting("Toxicity Difference Change Per Update", "Side Environment 1 Properties")]
        [DoubleSettingValueLimits(-1, 1)]
        public double S1EnvToxicityCPU { get; set; } = 0;
        [Setting("Ground Nutrient Content Difference Change Per Update", "Side Environment 1 Properties")]
        [DoubleSettingValueLimits(-1, 1)]
        public double S1EnvGroundNutrientContentCPU { get; set; } = 0;

        [Setting("Reverse Difference Value Changing", "Side Environment 1 Properties")]
        public bool S1EnvReverseChanging { get; set; } = true;

        #endregion
      
        #region Side 2 Env

        [Setting("Temperature Difference From Base", "Side Environment 2 Properties")]
        [DoubleSettingValueLimits(-100, 100)]
        public double S2EnvTemperature { get; set; } = 0;
        [Setting("Toxicity Difference From Base", "Side Environment 2 Properties")]
        [DoubleSettingValueLimits(-100, 100)]
        public double S2EnvToxicity { get; set; } = 10;
        [Setting("Ground Nutrient Content Difference From Base", "Side Environment 2 Properties")]
        [DoubleSettingValueLimits(-20, 20)]
        public double S2EnvGroundNutrientContent { get; set; } = 0;

        [Setting("Temperature Max Difference From Base", "Side Environment 2 Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double S2EnvTemperatureMaxDiff { get; set; } = 0;
        [Setting("Toxicity Max Difference From Base", "Side Environment 2 Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double S2EnvToxicityMaxDiff { get; set; } = 10;
        [Setting("Ground Nutrient Content Max Difference From Base", "Side Environment 2 Properties")]
        [DoubleSettingValueLimits(0, 20)]
        public double S2EnvGroundNutrientContentMaxDiff { get; set; } = 0;

        [Setting("Temperature Difference Change Per Update", "Side Environment 2 Properties")]
        [DoubleSettingValueLimits(-1, 1)]
        public double S2EnvTemperatureCPU { get; set; } = 0;
        [Setting("Toxicity Difference Change Per Update", "Side Environment 2 Properties")]
        [DoubleSettingValueLimits(-1, 1)]
        public double S2EnvToxicityCPU { get; set; } = -1d/600d;
        [Setting("Ground Nutrient Content  DifferenceChange Per Update", "Side Environment 2 Properties")]
        [DoubleSettingValueLimits(-1, 1)]
        public double S2EnvGroundNutrientContentCPU { get; set; } = 0;

        [Setting("Reverse Difference Value Changing", "Side Environment 2 Properties")]
        public bool S2EnvReverseChanging { get; set; } = true;

        #endregion

        #endregion

        #region Plant Starting Properties

        public PlantProperties StartingPlantProperties { get; } = new PlantProperties
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
        public double AdultSizePP { get => StartingPlantProperties.AdultSize.Value; set => StartingPlantProperties.AdultSize.Value = value; }

        [Setting("Mutation Strength", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0.5, 40)]
        public double MutationStrengthPP { get => StartingPlantProperties.MutationStrength.Value; set => StartingPlantProperties.MutationStrength.Value = value; }

        [Setting("Reproduction Energy", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double ReproductionEnergyPP { get => StartingPlantProperties.ReproductionEnergy.Value; set => StartingPlantProperties.ReproductionEnergy.Value = value; }

        [Setting("Offspring Amount", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 10)]
        public double OffspringAmountPP { get => StartingPlantProperties.OffspringAmount.Value; set => StartingPlantProperties.OffspringAmount.Value = value; }


        [Setting("Backup Energy", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double BackupEnergyPP { get => StartingPlantProperties.BackupEnergy.Value; set => StartingPlantProperties.BackupEnergy.Value = value; }

        [Setting("Optimal Temperature", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 80)]
        public double OptimalTemperaturePP { get => StartingPlantProperties.OptimalTemperature.Value; set => StartingPlantProperties.OptimalTemperature.Value = value; }

        [Setting("Temperature Change Resistance", "Plant Starting Properties")]
        [DoubleSettingValueLimits(1, 100)]
        public double TemperatureChangeResistancePP { get => StartingPlantProperties.TemperatureChangeResistance.Value; set => StartingPlantProperties.TemperatureChangeResistance.Value = value; }

        [Setting("Environment Toxicity Resistance", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double EnvironmentToxicityResistancePP { get => StartingPlantProperties.EnvironmentToxicityResistance.Value; set => StartingPlantProperties.EnvironmentToxicityResistance.Value = value; }

        [Setting("Toxicity", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double ToxicityPP { get => StartingPlantProperties.Toxicity.Value; set => StartingPlantProperties.Toxicity.Value = value; }

        [Setting("Reproduction Area", "Plant Starting Properties")]
        [DoubleSettingValueLimits(1, 300)]
        public double ReproductionAreaPP { get => StartingPlantProperties.ReproductionArea.Value; set => StartingPlantProperties.ReproductionArea.Value = value; }

        [Setting("Energy Production In Low Nutrient Grounds", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 20)]
        public double EnergyProductionInLowNutrientGroundsPP { get => StartingPlantProperties.EnergyProductionInLowNutrientGrounds.Value; set => StartingPlantProperties.EnergyProductionInLowNutrientGrounds.Value = value; }

        #endregion

        #region Animal Starting Properties

        public AnimalProperties StartingAnimalProperties { get; } = new AnimalProperties
        {
            AdultSize = new Property(20, 1, 1000, 50),
            MutationStrength = new Property(1, 0.5, 40, 5),
            ReproductionEnergy = new Property(50, 0, 100, 50),
            OffspringAmount = new Property(2, 1, 10, 3),
            BackupEnergy = new Property(20, 0, 100, 20),
            OptimalTemperature = new Property(30, 0, 80, 50),
            TemperatureChangeResistance = new Property(5, 1, 100, 10),
            EnvironmentToxicityResistance = new Property(0, 0, 100, 10),
            MovementSpeed = new Property(20, 0, 60, 10),
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
        public double AdultSizeAP { get => StartingAnimalProperties.AdultSize.Value; set => StartingAnimalProperties.AdultSize.Value = value; }

        [Setting("Mutation Strength", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0.5, 40)]
        public double MutationStrengthAP { get => StartingAnimalProperties.MutationStrength.Value; set => StartingAnimalProperties.MutationStrength.Value = value; }

        [Setting("Offspring Amount", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 10)]
        public double OffspringAmountAP { get => StartingAnimalProperties.OffspringAmount.Value; set => StartingAnimalProperties.OffspringAmount.Value = value; }

        [Setting("Reproduction Energy", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double ReproductionEnergyAP { get => StartingAnimalProperties.ReproductionEnergy.Value; set => StartingAnimalProperties.ReproductionEnergy.Value = value; }

        [Setting("Backup Energy", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double BackupEnergyAP { get => StartingAnimalProperties.BackupEnergy.Value; set => StartingAnimalProperties.BackupEnergy.Value = value; }

        [Setting("Optimal Temperature", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 80)]
        public double OptimalTemperatureAP { get => StartingAnimalProperties.OptimalTemperature.Value; set => StartingAnimalProperties.OptimalTemperature.Value = value; }

        [Setting("Temperature Change Resistance", "Animal Starting Properties")]
        [DoubleSettingValueLimits(1, 100)]
        public double TemperatureChangeResistanceAP { get => StartingAnimalProperties.TemperatureChangeResistance.Value; set => StartingAnimalProperties.TemperatureChangeResistance.Value = value; }

        [Setting("Environment Toxicity Resistance", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double EnvironmentToxicityResistanceAP { get => StartingAnimalProperties.EnvironmentToxicityResistance.Value; set => StartingAnimalProperties.EnvironmentToxicityResistance.Value = value; }

        [Setting("Movement Speed", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 40)]
        public double MovementSpeedAP { get => StartingAnimalProperties.MovementSpeed.Value; set => StartingAnimalProperties.MovementSpeed.Value = value; }

        [Setting("Carnivority Tendency", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double CarnivorityTendencyAP { get => StartingAnimalProperties.CarnivorityTendency.Value; set => StartingAnimalProperties.CarnivorityTendency.Value = value; }

        [Setting("Eyesight", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double EyesightAP { get => StartingAnimalProperties.Eyesight.Value; set => StartingAnimalProperties.Eyesight.Value = value; }

        [Setting("Offensive Capability", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 20)]
        public double OffensiveCapabilityAP { get => StartingAnimalProperties.OffensiveCapability.Value; set => StartingAnimalProperties.OffensiveCapability.Value = value; }

        [Setting("Defensive Capability", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 20)]
        public double DefensiveCapabilityAP { get => StartingAnimalProperties.DefensiveCapability.Value; set => StartingAnimalProperties.DefensiveCapability.Value = value; }

        [Setting("Threat Consideration Limit", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 40)]
        public double ThreatConsiderationLimitAP { get => StartingAnimalProperties.ThreatConsiderationLimit.Value; set => StartingAnimalProperties.ThreatConsiderationLimit.Value = value; }

        [Setting("Plant Toxicity Resistance", "Animal Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double PlantToxicityResistanceAP { get => StartingAnimalProperties.PlantToxicityResistance.Value; set => StartingAnimalProperties.PlantToxicityResistance.Value = value; }

        [Setting("Food Digesting Capability", "Animal Starting Properties")]
        [DoubleSettingValueLimits(1, 100)]
        public double FoodDigestingCapabilityAP { get => StartingAnimalProperties.FoodDigestingCapability.Value; set => StartingAnimalProperties.FoodDigestingCapability.Value = value; }

        #endregion
    }
}