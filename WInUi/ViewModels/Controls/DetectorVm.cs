using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using Windows.UI;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Dispatching;
using URE.Core.Contracts.Repositories;
using Microsoft.UI.Xaml;
using URE.Core.Models.Equipment;

namespace URE.ViewModels.Controls
{
    public class DetectorVm : ObservableRecipient
    {
        public IEnumerable<ISeries> Series { get; }


        public DetectorVm()
        {
            Series =
                GaugeGenerator.BuildSolidGauge(
                    new GaugeItem(0, series =>
                    {
                        series.Fill = new SolidColorPaint(SKColors.YellowGreen);
                        series.DataLabelsSize = 17;
                        series.DataLabelsFormatter = (s) => { return $"{DoseRate}"; };

                        series.DataLabelsPaint = new SolidColorPaint(SKColors.Black);
                        series.DataLabelsPosition = PolarLabelsPosition.ChartCenter;
                        series.InnerRadius = 35;
                    }),
                    new GaugeItem(GaugeItem.Background, series =>
                    {
                        series.InnerRadius = 35;
                        series.Fill = new SolidColorPaint(new SKColor(100, 181, 246, 90));
                    }));
        }

        private Visibility disableGridVisibility = Visibility.Collapsed;
        public Visibility DisableGridVisibility { get => disableGridVisibility; set => SetProperty(ref disableGridVisibility, value); }

        private Visibility toggleSwitchVisibility = Visibility.Visible;
        public Visibility ToggleSwitchVisibility { get => toggleSwitchVisibility; set { SetProperty(ref toggleSwitchVisibility, value); } }

        private Visibility noActiveTextVisibility = Visibility.Collapsed;
        public Visibility NoActiveTextVisibility { get => noActiveTextVisibility; set { SetProperty(ref noActiveTextVisibility, value); } }

        private Brush borderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
        public Brush BorderColorBrush { get => borderColorBrush; set => SetProperty(ref borderColorBrush, value); }

        public Color BorderColor { get; set; } = new Color();

        private bool toggleSwitchEnabled = true;
        public bool ToggleSwitchEnabled { get => toggleSwitchEnabled; set => SetProperty(ref toggleSwitchEnabled, value); }

        public SolidColorPaint ProgressBarColor 
        {
            get 
            {
                var series = Series.First() as PieSeries<LiveChartsCore.Defaults.ObservableValue>;
                return (SolidColorPaint)series!.Fill!;
            }
            set
            {
                var series = Series.First() as PieSeries<LiveChartsCore.Defaults.ObservableValue>;
                series!.Fill = value;
            } 
        }

        private bool enabled;
        public bool Enabled { get => enabled; 
            set 
            { 
                if (!value) 
                { 
                    DoseRate = 0;
                    DisableGridVisibility = Visibility.Visible;
                    BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
                }
                else
                {
                    DisableGridVisibility = Visibility.Collapsed;
                    BorderColorBrush = new SolidColorBrush(BorderColor);
                }
                
                SetProperty(ref enabled, value);
                OnEnabledChange?.Invoke(Enabled);
            } 
        }

        public event Action<bool> OnEnabledChange;

        private double doseRate;
        public double DoseRate { 
            get 
            {
                return doseRate;
            } 
            set 
            {
                if (!Enabled) return;
                var series = Series.First() as PieSeries<LiveChartsCore.Defaults.ObservableValue>;
                var val = series!.Values!.First();

                val.Value = value > MaxValue ? Math.Round(MaxValue, 3) : Math.Round(value, 3);
                SetProperty(ref doseRate, Math.Round(value, 3));
                ChangeColors();
            }
        }

        public string DetectorName { get; set; } = string.Empty;

        public GMSettings GMSettings { get; set; }

        public string Units { get; set; }
        
        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        public int DetectorNumber { get; set; }

        private void ChangeColors()
        {
            if (GMSettings.GetValueState(DoseRate, DetectorNumber) == SettingsValueState.High)
            {
                ProgressBarColor = new SolidColorPaint(new SKColor(253, 237, 102));
            }
            else if (GMSettings.GetValueState(DoseRate, DetectorNumber) == SettingsValueState.Critical)
            {
                ProgressBarColor = new SolidColorPaint(new SKColor(255, 102, 102));
            }
            else
            {
                ProgressBarColor = new SolidColorPaint(new SKColor(167, 224, 159));
            }
        }
    }
}
