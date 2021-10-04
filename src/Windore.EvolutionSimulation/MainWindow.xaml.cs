using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using Windore.Simulations2D.GUI;

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
            Simulation.Ins.OpenSettingsWindow();
        }

        private void OnStartBtnClick(object sender, RoutedEventArgs e)
        {
            simWindow = Simulation.Ins.OpenSimulationWindow();

            simWindow.SidePanelWidth = 310;
            simWindow.Show();

            simWindow.Closed += (_, __) => Simulation.Ins.DataWindowManager.CloseWindows();

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
                Text = $"Plant Amount: {Simulation.Ins.SimulationManager.PlantAmount}"
            });

            simWindow.AddSidePanelContent(new TextBlock()
            {
                Margin = new Thickness(5),
                FontSize = 16,
                Text = $"Animal Amount: {Simulation.Ins.SimulationManager.AnimalAmount}"
            });

            simWindow.AddSidePanelContent(new TextBlock()
            {
                Margin = new Thickness(5),
                FontSize = 16,
                Text = $"Duration: {Simulation.Ins.SimulationManager.Duration}"
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

            plantPanelBtn.Click += (_, __) =>
            {
                Simulation.Ins.DataWindowManager.OpenWindow(Simulation.Ins.SimulationManager, "All Plants", DataType.Plant);
            };
            animalPanelBtn.Click += (_, __) =>
            {
                Simulation.Ins.DataWindowManager.OpenWindow(Simulation.Ins.SimulationManager, "All Animals", DataType.Animal);
            };

            btnsPanel.Children.Add(plantPanelBtn);
            btnsPanel.Children.Add(animalPanelBtn);

            simWindow.AddSidePanelContent(btnsPanel);

            simWindow.AddSidePanelContent(Simulation.Ins.GetSelectedObjectPanel());
        }
    }
}