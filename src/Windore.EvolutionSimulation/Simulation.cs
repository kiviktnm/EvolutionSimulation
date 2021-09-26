using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using Windore.EvolutionSimulation.Objects;
using Windore.Settings.Base;
using Windore.Settings.GUI;
using Windore.Simulations2D;
using Windore.Simulations2D.GUI;
using Windore.Simulations2D.Util;
using Windore.Simulations2D.Util.SMath;

namespace Windore.EvolutionSimulation
{
    public class Simulation
    {
        public const double ENERGY_COEFFICIENT = 0.04d;
        private static Simulation ins;

        private Manager smng;
        private SettingsManager<SimulationSettings> settingsManager = new SettingsManager<SimulationSettings>();
        private SettingsWindow settingsWindow;

        public static Simulation Ins
        {
            get
            {
                if (ins == null)
                {
                    ins = new Simulation();
                    ins.settingsManager.SetSettingObject(ins.Settings);
                }
                return ins;
            }
        }

        public Manager SimulationManager
        {
            get
            {
                if (smng == null)
                {
                    SetUpSimulationManager();
                }
                return smng;
            }
        }

        public SimulationSettings Settings { get; private set; } = new SimulationSettings();
        public DataWindowManager DataWindowManager { get; } = new DataWindowManager();
        public SRandom SimulationRandom { get; private set; } = new SRandom(0);

        public SimulationWindow OpenSimulationWindow()
        {
            SimulationWindow simWindow = new SimulationWindow(SimulationManager, Settings.StartPaused);
            InitSimulation();
            return simWindow;
        }

        public void OpenSettingsWindow()
        {
            // Close a previos window before opening another
            if (settingsWindow != null)
                settingsWindow.Close();

            settingsWindow = new SettingsWindow(settingsManager);
            settingsWindow.Show();
        }

        public StackPanel GetSelectedObjectPanel()
        {
            StackPanel panel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical
            };

            if (SimulationManager.SimulationScene.SelectedSimulationObject == null || SimulationManager.SimulationScene.SelectedSimulationObject.IsRemoved)
            {
                panel.Children.Add(CreateNewSelectedPanelText("No Selection"));
                return panel;
            }

            switch (SimulationManager.SimulationScene.SelectedSimulationObject)
            {
                case Plant plnt:
                    panel.Children.Add(new TextBlock { Text = "Plant", Margin = new Avalonia.Thickness(5, 5, 5, 0), FontSize = 16, FontWeight = Avalonia.Media.FontWeight.Bold });
                    AddOrganismToPanel(panel, plnt);

                    foreach (KeyValuePair<string, Property> pair in plnt.Properties.Properties)
                    {
                        panel.Children.Add(CreateNewSelectedPanelText($"{pair.Key}: {Math.Round(pair.Value.Value, 3)}"));
                    }
                    break;
                case Animal animal:
                    panel.Children.Add(new TextBlock { Text = "Animal", Margin = new Avalonia.Thickness(5, 5, 5, 0), FontSize = 16, FontWeight = Avalonia.Media.FontWeight.Bold });
                    AddOrganismToPanel(panel, animal);
                    panel.Children.Add(CreateNewSelectedPanelText($"Stored Food: {Math.Round(animal.StoredFood, 3)}"));
                    panel.Children.Add(CreateNewSelectedPanelText($"Current Objective: {animal.CurrentObjective}"));

                    foreach (KeyValuePair<string, Property> pair in animal.Properties.Properties)
                    {
                        panel.Children.Add(CreateNewSelectedPanelText($"{pair.Key}: {Math.Round(pair.Value.Value, 3)}"));
                    }
                    break;
                case Objects.Environment env:
                    AddEnvironmentToPanel(panel, env);
                    break;
            }

            return panel;
        }

        private void AddOrganismToPanel(StackPanel panel, Organism organism)
        {
            panel.Children.Add(CreateNewSelectedPanelText($"Current Energy: {Math.Round(organism.CurrentEnergy, 3)}"));
            panel.Children.Add(CreateNewSelectedPanelText($"Energy Production: {Math.Round(organism.EnergyProduction, 3)}"));
            panel.Children.Add(CreateNewSelectedPanelText($"Energy Consumption: {Math.Round(organism.EnergyConsumption, 3)}"));
            panel.Children.Add(CreateNewSelectedPanelText($"Energy Storing Capacity: {Math.Round(organism.EnergyStoringCapacity, 3)}"));
            panel.Children.Add(CreateNewSelectedPanelText($"Current Size: {Math.Round(organism.CurrentSize, 3)}"));
            panel.Children.Add(CreateNewSelectedPanelText($"Current Age: {organism.Age}"));
        }

        private void AddEnvironmentToPanel(StackPanel panel, Objects.Environment env)
        {
            panel.Children.Add(new TextBlock { Text = "Environment", Margin = new Avalonia.Thickness(5, 5, 5, 0), FontSize = 16, FontWeight = Avalonia.Media.FontWeight.Bold });
            panel.Children.Add(CreateNewSelectedPanelText($"Toxicity: {Math.Round(env.Toxicity.Value, 3)}"));
            panel.Children.Add(CreateNewSelectedPanelText($"Temperature: {Math.Round(env.Temperature.Value, 3)}"));
            panel.Children.Add(CreateNewSelectedPanelText($"Ground Nutrient Content: {Math.Round(env.GroundNutrientContent.Value, 3)}"));
            panel.Children.Add(CreateNewSelectedPanelText($"Plant Amount in Env: {env.PlantAmount}"));
            panel.Children.Add(CreateNewSelectedPanelText($"Animal Amount in Env: {env.AnimalAmount}"));

            Button plantPanelBtn = new Button
            {
                Margin = new Avalonia.Thickness(5),
                Height = 30,
                Content = "Open Env Plants Panel"
            };

            Button animalPanelBtn = new Button
            {
                Margin = new Avalonia.Thickness(5),
                Height = 30,
                Content = "Open Env Animals Panel"
            };

            plantPanelBtn.Click += (_, __) =>
            {
                DataWindowManager.OpenWindow(env.PlantsData, $"{env.Name} Environment Plants");
            };

            animalPanelBtn.Click += (_, __) =>
            {
                DataWindowManager.OpenWindow(env.AnimalsData, $"{env.Name} Environment Animals");
            };

            panel.Children.Add(plantPanelBtn);
            panel.Children.Add(animalPanelBtn);
        }

        private void SetUpSimulationManager()
        {
            SimulationScene scene = new SimulationScene(Settings.SimulationSceneSideLength, Settings.SimulationSceneSideLength);
            smng = new Manager(scene);

            SetUpEnvs();
            SetUpScene(scene);

            // This must be called last, because all objects that have loggable properties must have been added to the simulation.
            smng.SetUpLogging();
        }

        private void SetUpScene(SimulationScene scene)
        {
            Point startingPoint = new Point(scene.Width * 0.50, scene.Height * 0.66);

            // Add 150 randomly placed plants to the scene
            for (int i = 0; i < 150; i++)
            {
                // Bruteforce a position within an environment
                Point pos;
                do
                {
                    double range = Settings.SimulationSceneSideLength * 0.2075d;
                    double posX = SimulationRandom.Double(startingPoint.X - range, startingPoint.X + range);
                    double posY = SimulationRandom.Double(startingPoint.Y - range, startingPoint.Y + range);

                    pos = new Point(posX, posY);
                } while (SimulationManager.GetEnvironment(pos) == null);


                double startingEnergy = SimulationRandom.Double(2, 40);
                Plant startingPlant = new Plant(pos, startingEnergy, startingEnergy / 2d, Settings.StartingPlantProperties);

                scene.Add(startingPlant);
            }

            // One plant is placed directly to the center. This plant will serve as food to the starting animal
            Plant startingCenterPlant = new Plant(startingPoint, 20, 10, Settings.StartingPlantProperties);
            scene.Add(startingCenterPlant);

            // Add starting animal
            Animal startingAnimal = new Animal(startingPoint, 20, 10, Settings.StartingAnimalProperties)
            {
                StoredFood = 10
            };
            scene.Add(startingAnimal);
        }

        private void SetUpEnvs()
        {
            double sideLength = Settings.SimulationSceneSideLength;
            double envSize = sideLength * 0.415d;

            SimulationManager.Envs[0] = new Objects.Environment(new Point(sideLength * 0.50, sideLength * 0.66), envSize)
            {
                Toxicity = new ChangingVariable(0, 0, 0, 0, SimulationManager.BaseEnvToxicity),
                Temperature = new ChangingVariable(0, 0, 0, 0, SimulationManager.BaseEnvTemperature),
                GroundNutrientContent = new ChangingVariable(0, 0, 10, -1d / 3000d, SimulationManager.BaseEnvGroundNutrientContent),
                Name = "Ground Nutrients"
            };

            SimulationManager.Envs[1] = new Objects.Environment(new Point(sideLength * 0.75, sideLength * 0.33), envSize)
            {
                Toxicity = new ChangingVariable(0, 0, 0, 0, SimulationManager.BaseEnvToxicity),
                Temperature = new ChangingVariable(10, 0, 10, 1 / 3000d, SimulationManager.BaseEnvTemperature),
                GroundNutrientContent = new ChangingVariable(0, 0, 0, 0, SimulationManager.BaseEnvGroundNutrientContent),
                Name = "Temperature"

            };

            SimulationManager.Envs[2] = new Objects.Environment(new Point(sideLength * 0.25, sideLength * 0.33), envSize)
            {
                Toxicity = new ChangingVariable(10, 0, 10, -1d / 3000d, SimulationManager.BaseEnvToxicity),
                Temperature = new ChangingVariable(0, 0, 0, 0, SimulationManager.BaseEnvTemperature),
                GroundNutrientContent = new ChangingVariable(0, 0, 0, 0, SimulationManager.BaseEnvGroundNutrientContent),
                Name = "Toxicity"
            };

            SimulationManager.SimulationScene.AddAll(SimulationManager.Envs);
        }

        private void InitSimulation()
        {
            InitSRandom();
            if (settingsWindow != null) settingsWindow.Close();
            WriteSettingsFile();
        }

        private void InitSRandom()
        {
            if (Settings.RandomnessSeed == 0)
            {
                SimulationRandom = new SRandom();
            }
        }

        private void WriteSettingsFile()
        {
            Directory.CreateDirectory(Settings.SimulationLogDirectory);
            string text = settingsManager.GenerateSettingsString();

            string fileName = Path.Combine(Settings.SimulationLogDirectory, "simulation-settings-file");

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

        private TextBlock CreateNewSelectedPanelText(string text)
        {
            return new TextBlock()
            {
                Text = text,
                Margin = new Avalonia.Thickness(5, 5, 5, 0),
                FontSize = 14,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
        }
    }
}