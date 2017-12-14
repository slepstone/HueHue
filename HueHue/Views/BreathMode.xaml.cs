﻿using HueHue.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Media = System.Windows.Media;

namespace HueHue.Views
{
    /// <summary>
    /// Interaction logic for BreathMode.xaml
    /// </summary>
    public partial class BreathMode : UserControl
    {
        Thread _workerThread;
        CancellationTokenSource _cancellationTokenSource;
        bool increase;
        int brightness = 0;

        public BreathMode()
        {
            InitializeComponent();

            gridMain.DataContext = App.settings;

            colorPanel.SelectedColorBrush = new Media.SolidColorBrush(Media.Color.FromArgb(0, Effects.Colors[0].R, Effects.Colors[0].G, Effects.Colors[0].B));
            colorPanel.InitialColorBrush = new Media.SolidColorBrush(Media.Color.FromArgb(0, Effects.Colors[0].R, Effects.Colors[0].G, Effects.Colors[0].B));

            _cancellationTokenSource = new CancellationTokenSource();
            _workerThread = new Thread(BackgroundWorker_DoWork) { Name = "BreathEffect", IsBackground = true };
            _workerThread.Start(_cancellationTokenSource.Token);

            App.settings.Brightness = 0;
        }

        private void BackgroundWorker_DoWork(object tokenObject)
        {
            var cancellationToken = (CancellationToken)tokenObject;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (increase)
                    {
                        brightness += App.settings.Length;
                    }
                    else
                    {
                        brightness -= App.settings.Length;
                    }

                    if (brightness <= 0)
                    {
                        increase = true;
                        if (App.settings.BreathRandomize)
                        {
                            Effects.Colors[0] = new LEDBulb();
                            Effects.FixedColor();
                            brightness = 0;
                        }
                    }
                    else if (brightness >= 255)
                    {
                        brightness = 255;
                        increase = false;
                    }

                    App.settings.Brightness = (byte)brightness;
                    Task.Delay(App.settings.Speed, cancellationToken).Wait(cancellationToken);

                }
                catch (Exception)
                {
                }
            }
        }

        private void DefaultColor_ColorChanged(object sender, ColorTools.ColorControlPanel.ColorChangedEventArgs e)
        {
            Effects.Colors[0] = (LEDBulb)e.CurrentColor;

            Effects.FixedColor();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;
            _workerThread.Join();
            _workerThread = null;
        }

        private void toggle_mode_Unchecked(object sender, RoutedEventArgs e)
        {
            if (toggle_mode.IsChecked == true)
            {
                colorPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                colorPanel.Visibility = Visibility.Visible;
            }
        }
    }
}
