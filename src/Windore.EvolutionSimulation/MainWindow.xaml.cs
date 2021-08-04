using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using Windore.Simulations2D.GUI;
using Avalonia.Threading;

namespace Windore.EvolutionSimulation
{
    public class MainWindow : Window
    {
        private SimulationWindow simWindow;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnSettingsBtnClick(object sender, RoutedEventArgs e) 
        {
            SimulationSettings.Instance.OpenSettingsWindow();
        }

        private void OnStartBtnClick(object sender, RoutedEventArgs e) 
        {
            simWindow = new SimulationWindow(SimulationSettings.Instance.SimulationManager);
            simWindow.SidePanelWidth = 250;
            simWindow.Show();

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.5d)
            };
            timer.Tick += (_, __) => UpdateSidePanel();
            timer.Start();

            Close();
        }

        private void UpdateSidePanel()
        {
            simWindow.SetSidePanelContent(new TextBlock()
            {
                Margin = new Thickness(5),
                FontSize = 16,
                Text = $"Plant Amount: {SimulationSettings.Instance.SimulationManager.PlantAmount}"
            });

            simWindow.AddSidePanelContent(new TextBlock()
            {
                Margin = new Thickness(5),
                FontSize = 16,
                Text = $"Animal Amount: {0}"
            });

            StackPanel btnsPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Height = 30
            };

            Button plantPanelBtn = new Button
            {
                Margin = new Thickness(5),
                Height = 30,
                Content = "Open All Plants Panel"
            };

            Button animalPanelBtn = new Button
            {
                Margin = new Thickness(5),
                Height = 30,
                Content = "Open All Animals Panel"
            };

            plantPanelBtn.Click += (_, __) => OpenPlantPanel();
            animalPanelBtn.Click += (_, __) => OpenAnimalPanel();

            btnsPanel.Children.Add(plantPanelBtn);
            btnsPanel.Children.Add(animalPanelBtn);

            simWindow.AddSidePanelContent(btnsPanel);

            simWindow.AddSidePanelContent(SimulationSettings.Instance.SimulationManager.SelectedObjectPanel);
        }

        private void OpenPlantPanel() 
        {
        }

        private void OpenAnimalPanel() 
        {
        }
    }
}