using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using Windore.Simulations2D.Data;
using Windore.Simulations2D.GUI;

namespace Windore.EvolutionSimulation
{
    public class DataWindowManager
    {
        private readonly List<Window> windows = new List<Window>();

        public void OpenWindow(Dictionary<string, DataCollector.Data> data, string title)
        {
            SimulationDataView dataView = new SimulationDataView
            {
                Data = data,
                HideSingleValueData = false,
                Rounding = true
            };

            Window panelWindow = new Window
            {
                Width = 1000,
                Height = 800,
                Title = title,
                Content = dataView
            };

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (_, __) => dataView.Data = data;
            timer.Start();

            panelWindow.Closed += (_, __) => timer.Stop();

            panelWindow.Show();

            windows.Add(panelWindow);
        }

        public void CloseWindows()
        {
            foreach (Window window in windows)
            {
                window.Close();
            }
        }
    }
}
