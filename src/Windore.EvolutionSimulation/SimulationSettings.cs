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
        [Setting("Simulation Area Side Length", "General")]
        [DoubleSettingValueLimits(100, 2000)]
        public double SimulationSceneSideLength { get; set; } = 10_000;

        [Setting("Mutation Probability", "General")]
        [DoubleSettingValueLimits(0, 100)]
        public double MutationProbability { get; set; } = 5;

        [Setting("Randomness Seed (0 for random seed)", "General")]
        public int RandomnessSeed { get; set; } = 0;

        [Setting("Simulation Log Directory", "General")]
        [StringSettingIsPath]
        public string SimulationLogDirectory { get; set; } = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "/EvolutionSimulation/");

        #region Plant Starting Properties

        private PlantProperties startingPlantProperties = new PlantProperties
        {
            AdultSize = new Property(20, 1, 1000, 50),
            MutationStrength = new Property(0.1, 0.001, 1, 0.05),
            GrowthRate = new Property(0.01, 0.001, 10, 0.1),
            OffspringAmount = new Property(1, 1, 10, 3),
            ReproductionEnergy = new Property(50, 0, 100, 50),
            BackupEnergy = new Property(20, 0, 100, 20),
            OptimalTemperature = new Property(30, 0, 80, 50),
            TemperatureChangeResistance = new Property(5, 1, 100, 10),
            EnvironmentToxicityResistance = new Property(0, 0, 100, 10),
            Toxicity = new Property(0, 0, 100, 10),
            ReproductionArea = new Property(5, 1, 200, 20),
            EnergyProductionInLowNutrientGrounds = new Property(0, 0, 20, 5)
        };

        [Setting("Adult Size", "Plant Starting Properties")]
        [DoubleSettingValueLimits(1, 1000)]
        public double AdultSizePP { get => startingPlantProperties.AdultSize.Value; set => startingPlantProperties.AdultSize.Value = value; }

        [Setting("Mutation Strength", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0.001, 1)]
        public double MutationStrengthPP { get => startingPlantProperties.MutationStrength.Value; set => startingPlantProperties.MutationStrength.Value = value; }

        [Setting("Growth Rate", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0.001, 10)]
        public double GrowthRatePP { get => startingPlantProperties.GrowthRate.Value; set => startingPlantProperties.GrowthRate.Value = value; }

        [Setting("Offspring Amount", "Plant Starting Properties")]
        [DoubleSettingValueLimits(1, 10)]
        public double OffspringAmountPP { get => startingPlantProperties.OffspringAmount.Value; set => startingPlantProperties.OffspringAmount.Value = value; }

        [Setting("Reproduction Energy", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 100)]
        public double ReproductionEnergyPP { get => startingPlantProperties.ReproductionEnergy.Value; set => startingPlantProperties.ReproductionEnergy.Value = value; }

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
        [DoubleSettingValueLimits(1, 200)]
        public double ReproductionAreaPP { get => startingPlantProperties.ReproductionArea.Value; set => startingPlantProperties.ReproductionArea.Value = value; }

        [Setting("Energy Production In Low Nutrient Grounds", "Plant Starting Properties")]
        [DoubleSettingValueLimits(0, 20)]
        public double EnergyProductionInLowNutrientGroundsPP { get => startingPlantProperties.EnergyProductionInLowNutrientGrounds.Value; set => startingPlantProperties.EnergyProductionInLowNutrientGrounds.Value = value; }

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

        public SRandom SimulationRandom { get; }
        private Manager smng;
        public Manager SimulationManager
        {
            get
            {
                if (smng == null) 
                {
                    SimulationScene scene = new SimulationScene(Instance.SimulationSceneSideLength, Instance.SimulationSceneSideLength);
                    smng = new Manager(scene);

                    Point plantPoint = new Point(scene.Width * 0.50, scene.Height * 0.66);
                    Plant startingPlant = new Plant(plantPoint, 10, startingPlantProperties);

                    scene.Add(startingPlant);

                    smng.SetUpEnvs();

                    // Logging must be set up only after all object types that will be logged are added to the simulation.
                    smng.SetUpLogging();

                }
                return smng;
            }
        }

        private SettingsManager<SimulationSettings> manager = new SettingsManager<SimulationSettings>();

        private SimulationSettings() 
        {
            manager.SetSettingObject(this);

            if (RandomnessSeed == 0) 
            {
                SimulationRandom = new SRandom();
            }
            else 
            {
                SimulationRandom = new SRandom(RandomnessSeed);
            }
        }

        public void OpenSettingsWindow() 
        {
            SettingsWindow window = new SettingsWindow(manager);
            window.Show();
        }
    }
}