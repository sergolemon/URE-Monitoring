using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using System.Collections.ObjectModel;
using URE.Contracts.Services;
using URE.Core.Contracts.Repositories;
using URE.Core.Models.Meteo;
using URE.Extensions;
using URE.ViewModels.Controls;
using Windows.UI;

namespace URE.ViewModels;

public class MainViewModel : ObservableRecipient
{
    private readonly IMeteoStreamService _meteoStreamService;
    private readonly IGMSettingsRepository _gmSettingsRepository;
    private ShellViewModel _shellViewModel;

    public HistogramVm Histogram { get; set; }

    public ObservableCollection<MeteoData> AutoMeteoData { get; set; } = new();
    public ObservableCollection<MeteoData> ManualMeteoData { get; set; } = new();

    public DetectorVm Detector1 { get; set; }
    public DetectorVm Detector2 { get; set; }
    public DetectorVm Detector3 { get; set; }
    public DetectorVm Detector4 { get; set; }
    public DetectorVm Detector5 { get; set; }
    public DetectorVm Detector6 { get; set; }

    public MainViewModel(IMeteoStreamService meteoStreamService, IGMSettingsRepository gmSettingsRepository, IGPSSettingsRepository gpsSettingsRepository)
    {
        //AutoMeteoData.CollectionChanged += (s, e) => {  };
        _meteoStreamService = meteoStreamService;
        _gmSettingsRepository = gmSettingsRepository;

        var gmSettings = _gmSettingsRepository.GetGMSettings();
        var gpsSettings = gpsSettingsRepository.GetGPSSettingsOrNull();
        
        var detector1Settings = gmSettings.DetectorSettings.ElementAtOrDefault(0);
        var detector2Settings = gmSettings.DetectorSettings.ElementAtOrDefault(1);
        var detector3Settings = gmSettings.DetectorSettings.ElementAtOrDefault(2);
        var detector4Settings = gmSettings.DetectorSettings.ElementAtOrDefault(3);
        var detector5Settings = gmSettings.DetectorSettings.ElementAtOrDefault(4);
        var detector6Settings = gmSettings.DetectorSettings.ElementAtOrDefault(5);

        var moveSpeedColor = ColorExtensions.FromInt(gpsSettings?.MoveSpeedColor ?? 0);

        var detector1LineColor = ColorExtensions.FromInt(detector1Settings?.Color ?? 0);
        var detector2LineColor = ColorExtensions.FromInt(detector2Settings?.Color ?? 0);
        var detector3LineColor = ColorExtensions.FromInt(detector3Settings?.Color ?? 0);
        var detector4LineColor = ColorExtensions.FromInt(detector4Settings?.Color ?? 0);
        var detector5LineColor = ColorExtensions.FromInt(detector5Settings?.Color ?? 0);
        var detector6LineColor = ColorExtensions.FromInt(detector6Settings?.Color ?? 0);

        Detector1 = new DetectorVm();
        Detector1.DetectorName = detector1Settings?.Name;
        Detector1.GMSettings = gmSettings;
        Detector1.Units = "мкЗв/год";
        Detector1.MinValue = 0;
        Detector1.MaxValue = detector1Settings.CriticalValue;
        Detector1.DetectorNumber = 0;
        Detector1.BorderColor = detector1LineColor;
        //Detector1.Enabled = detector1Settings?.IsEnabled ?? false;
        Detector1.ToggleSwitchVisibility = detector1Settings?.IsEnabled ?? false ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        Detector1.NoActiveTextVisibility = detector1Settings?.IsEnabled ?? false ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
        Detector2 = new DetectorVm();
        Detector2.DetectorName = detector2Settings?.Name;
        Detector2.GMSettings = gmSettings;
        //Detector2.Enabled = gmSettings.DetectorSettings.ElementAtOrDefault(1)?.IsEnabled ?? false;
        Detector2.Units = "мкЗв/год";
        Detector2.MinValue = 0;
        Detector2.MaxValue = detector2Settings.CriticalValue;
        Detector2.DetectorNumber = 1;
        Detector2.BorderColor = detector2LineColor;
        //Detector2.Enabled = detector2Settings?.IsEnabled ?? false;
        Detector2.ToggleSwitchVisibility = detector2Settings?.IsEnabled ?? false ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        Detector2.NoActiveTextVisibility = detector2Settings?.IsEnabled ?? false ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
        Detector3 = new DetectorVm();
        Detector3.DetectorName = detector3Settings?.Name;
        Detector3.GMSettings = gmSettings;
        //Detector3.Enabled = gmSettings.DetectorSettings.ElementAtOrDefault(2)?.IsEnabled ?? false;
        Detector3.Units = "мкЗв/год";
        Detector3.MinValue = 0;
        Detector3.MaxValue = detector3Settings.CriticalValue;
        Detector3.DetectorNumber = 2;
        Detector3.BorderColor = detector3LineColor;
        //Detector3.Enabled = detector3Settings?.IsEnabled ?? false;
        Detector3.ToggleSwitchVisibility = detector3Settings?.IsEnabled ?? false ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        Detector3.NoActiveTextVisibility = detector3Settings?.IsEnabled ?? false ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
        Detector4 = new DetectorVm();
        Detector4.DetectorName = detector4Settings?.Name;
        Detector4.GMSettings = gmSettings;
        //Detector4.Enabled = gmSettings.DetectorSettings.ElementAtOrDefault(3)?.IsEnabled ?? false;
        Detector4.Units = "мкЗв/год";
        Detector4.MinValue = 0;
        Detector4.MaxValue = detector4Settings.CriticalValue;
        Detector4.DetectorNumber = 3;
        Detector4.BorderColor = detector4LineColor;
        //Detector4.Enabled = detector4Settings?.IsEnabled ?? false;
        Detector4.ToggleSwitchVisibility = detector4Settings?.IsEnabled ?? false ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        Detector4.NoActiveTextVisibility = detector4Settings?.IsEnabled ?? false ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
        Detector5 = new DetectorVm();
        Detector5.DetectorName = detector5Settings?.Name;
        Detector5.GMSettings = gmSettings;
        //Detector5.Enabled = gmSettings.DetectorSettings.ElementAtOrDefault(4)?.IsEnabled ?? false;
        Detector5.Units = "мкЗв/год";
        Detector5.MinValue = 0;
        Detector5.MaxValue = detector5Settings.CriticalValue;
        Detector5.DetectorNumber = 4;
        Detector5.BorderColor = detector5LineColor;
        //Detector5.Enabled = detector5Settings?.IsEnabled ?? false;
        Detector5.ToggleSwitchVisibility = detector5Settings?.IsEnabled ?? false ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        Detector5.NoActiveTextVisibility = detector5Settings?.IsEnabled ?? false ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
        Detector6 = new DetectorVm();
        Detector6.DetectorName = detector6Settings?.Name;
        Detector6.GMSettings = gmSettings;
        //Detector6.Enabled = gmSettings.DetectorSettings.ElementAtOrDefault(5)?.IsEnabled ?? false;
        Detector6.Units = "мкЗв/год";
        Detector6.MinValue = 0;
        Detector6.MaxValue = detector6Settings.CriticalValue;
        Detector6.DetectorNumber = 5;
        Detector6.BorderColor = detector6LineColor;
        //Detector6.Enabled = detector6Settings?.IsEnabled ?? false;
        Detector6.ToggleSwitchVisibility = detector6Settings?.IsEnabled ?? false ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        Detector6.NoActiveTextVisibility = detector6Settings?.IsEnabled ?? false ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;

        var linesArr = new HistogramLineVm[]
        {
            new HistogramLineVm(new SKColor(detector1LineColor.R, detector1LineColor.G, detector1LineColor.B), Detector1.DetectorName),
            new HistogramLineVm(new SKColor(detector2LineColor.R, detector2LineColor.G, detector2LineColor.B), Detector2.DetectorName),
            new HistogramLineVm(new SKColor(detector3LineColor.R, detector3LineColor.G, detector3LineColor.B), Detector3.DetectorName),
            new HistogramLineVm(new SKColor(detector4LineColor.R, detector4LineColor.G, detector4LineColor.B), Detector4.DetectorName),
            new HistogramLineVm(new SKColor(detector5LineColor.R, detector5LineColor.G, detector5LineColor.B), Detector5.DetectorName),
            new HistogramLineVm(new SKColor(detector6LineColor.R, detector6LineColor.G, detector6LineColor.B), Detector6.DetectorName)
        };

        Histogram = new HistogramVm(5, linesArr, new HistogramLineVm(moveSpeedColor.R, moveSpeedColor.G, moveSpeedColor.B, "Швидкість руху"), gmSettings);
        Histogram.PointClicked += OnHistogramPointClicked;

        Detector1.OnEnabledChange += (enable) => { Histogram.SetVisibilityOfDoseRateLine(0, enable); };
        Detector2.OnEnabledChange += (enable) => { Histogram.SetVisibilityOfDoseRateLine(1, enable); };
        Detector3.OnEnabledChange += (enable) => { Histogram.SetVisibilityOfDoseRateLine(2, enable); };
        Detector4.OnEnabledChange += (enable) => { Histogram.SetVisibilityOfDoseRateLine(3, enable); };
        Detector5.OnEnabledChange += (enable) => { Histogram.SetVisibilityOfDoseRateLine(4, enable); };
        Detector6.OnEnabledChange += (enable) => { Histogram.SetVisibilityOfDoseRateLine(5, enable); };

        Detector1.Enabled = detector1Settings?.IsEnabled ?? false;
        Detector2.Enabled = detector2Settings?.IsEnabled ?? false;
        Detector3.Enabled = detector3Settings?.IsEnabled ?? false;
        Detector4.Enabled = detector4Settings?.IsEnabled ?? false;
        Detector5.Enabled = detector5Settings?.IsEnabled ?? false;
        Detector6.Enabled = detector6Settings?.IsEnabled ?? false;

        _meteoStreamService.OnDataChanged += MeteoDataChanged;
    }

    private void MeteoDataChanged(MeteoData data)
    {
        Detector1.DoseRate = data.D1Radiation ?? 0;
        Detector2.DoseRate = data.D2Radiation ?? 0;
        Detector3.DoseRate = data.D3Radiation ?? 0;
        Detector4.DoseRate = data.D4Radiation ?? 0;
        Detector5.DoseRate = data.D5Radiation ?? 0;
        Detector6.DoseRate = data.D6Radiation ?? 0;
    }

    private void RenderMeteoDataByStream(MeteoData meteoData, bool isAutoStream)
    {
        Histogram.AddXLabel((meteoData.Date + meteoData.Time).ToLongTimeString());

        if (isAutoStream) 
        {
            Histogram.AddDetectorDoseRatePoint(0, meteoData.Id, meteoData.D1Radiation.HasValue ? Math.Round(meteoData.D1Radiation.Value, 3) : null);
            Histogram.AddDetectorDoseRatePoint(1, meteoData.Id, meteoData.D2Radiation.HasValue ? Math.Round(meteoData.D2Radiation.Value, 3) : null);
            Histogram.AddDetectorDoseRatePoint(2, meteoData.Id, meteoData.D3Radiation.HasValue ? Math.Round(meteoData.D3Radiation.Value, 3) : null);
            Histogram.AddDetectorDoseRatePoint(3, meteoData.Id, meteoData.D4Radiation.HasValue ? Math.Round(meteoData.D4Radiation.Value, 3) : null);
            Histogram.AddDetectorDoseRatePoint(4, meteoData.Id, meteoData.D5Radiation.HasValue ? Math.Round(meteoData.D5Radiation.Value, 3) : null);
            Histogram.AddDetectorDoseRatePoint(5, meteoData.Id, meteoData.D6Radiation.HasValue ? Math.Round(meteoData.D6Radiation.Value, 3) : null);
            Histogram.AddMoveSpeedPoint(meteoData.Id, meteoData.Speed);

            AutoMeteoData.Insert(0, meteoData); 
        }
        else 
        {
            Histogram.AddManualDoseRatePoint(meteoData.Id, Math.Round(meteoData.ManualInputRadiation, 3));

            ManualMeteoData.Insert(0, meteoData); 
        }
    }

    public void OnNavigated(ShellViewModel shellViewModel)
    {
        _shellViewModel = shellViewModel;
        foreach (var item in _shellViewModel.AutoStreamMeteoDatas)
        {
            RenderMeteoDataByStream(item, true);
        }

        if (_shellViewModel.IsAutoStream == true || AutoMeteoData.Any()) Histogram.SetVisibilityOfMoveSpeedLine(_shellViewModel.MoveSpeedEnabled);
        else Histogram.SetVisibilityOfMoveSpeedLine(false);

        _shellViewModel.AutoStreamMeteoDatas.CollectionChanged += (s, e) => {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    RenderMeteoDataByStream((MeteoData)item, true);
                }
            }
            else if(e.OldItems == null)
            {
                AutoMeteoData.Clear();
                Histogram.ClearHistogram();
            }
        };

        foreach (var item in _shellViewModel.ManualStreamMeteoDatas)
        {
            RenderMeteoDataByStream(item, false);
        }

        _shellViewModel.ManualStreamMeteoDatas.CollectionChanged += (s, e) => {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    RenderMeteoDataByStream((MeteoData)item, false);
                }
            }
            else if (e.OldItems == null)
            {
                ManualMeteoData.Clear();
                Histogram.ClearHistogram();
            }
        };

        //if (shellViewModel.IsAutoStream == false)
        //{
        //Detector1.ToggleSwitchEnabled = false;
        //Detector1.DisableGridVisibility = Microsoft.UI.Xaml.Visibility.Visible;
        //Detector1.BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
        //Detector2.ToggleSwitchEnabled = false;
        //Detector2.DisableGridVisibility = Microsoft.UI.Xaml.Visibility.Visible;
        //Detector2.BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
        //Detector3.ToggleSwitchEnabled = false;
        //Detector3.DisableGridVisibility = Microsoft.UI.Xaml.Visibility.Visible;
        //Detector3.BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
        //Detector4.ToggleSwitchEnabled = false;
        //Detector4.DisableGridVisibility = Microsoft.UI.Xaml.Visibility.Visible;
        //Detector4.BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
        //Detector5.ToggleSwitchEnabled = false;
        //Detector5.DisableGridVisibility = Microsoft.UI.Xaml.Visibility.Visible;
        //Detector5.BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
        //Detector6.ToggleSwitchEnabled = false;
        //Detector6.DisableGridVisibility = Microsoft.UI.Xaml.Visibility.Visible;
        //Detector6.BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
        //}

        _shellViewModel.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName) 
            {
                case nameof(_shellViewModel.MoveSpeedEnabled): 
                    { 
                        if(_shellViewModel.IsAutoStream == true || AutoMeteoData.Any()) Histogram.SetVisibilityOfMoveSpeedLine(_shellViewModel.MoveSpeedEnabled); 
                        break; 
                    }
                case nameof(_shellViewModel.IsAutoStream): 
                    {
                        if (_shellViewModel.IsAutoStream == true)
                        {
                            AutoMeteoData.Clear();
                            ManualMeteoData.Clear();
                            Histogram.SetVisibilityOfMoveSpeedLine(_shellViewModel.MoveSpeedEnabled);
                        }
                        else if(_shellViewModel.IsAutoStream == false)
                        {
                            AutoMeteoData.Clear();
                            ManualMeteoData.Clear();
                            //Detector1.ToggleSwitchEnabled = false;
                            //Detector1.DoseRate = 0;
                            //Detector1.DisableGridVisibility = Microsoft.UI.Xaml.Visibility.Visible;
                            //Detector1.BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
                            //Detector2.ToggleSwitchEnabled = false;
                            //Detector2.DoseRate = 0;
                            //Detector2.DisableGridVisibility = Microsoft.UI.Xaml.Visibility.Visible;
                            //Detector2.BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
                            //Detector3.DoseRate = 0;
                            //Detector3.ToggleSwitchEnabled = false;
                            //Detector3.DisableGridVisibility = Microsoft.UI.Xaml.Visibility.Visible;
                            //Detector3.BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
                            //Detector4.DoseRate = 0;
                            //Detector4.ToggleSwitchEnabled = false;
                            //Detector4.DisableGridVisibility = Microsoft.UI.Xaml.Visibility.Visible;
                            //Detector4.BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
                            //Detector5.DoseRate = 0;
                            //Detector5.ToggleSwitchEnabled = false;
                            //Detector5.DisableGridVisibility = Microsoft.UI.Xaml.Visibility.Visible;
                            //Detector5.BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
                            //Detector6.DoseRate = 0;
                            //Detector6.ToggleSwitchEnabled = false;
                            //Detector6.DisableGridVisibility = Microsoft.UI.Xaml.Visibility.Visible;
                            //Detector6.BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
                            Histogram.SetVisibilityOfMoveSpeedLine(false);
                        }
                        //else
                        //{
                        //    Detector1.ToggleSwitchEnabled = true;
                        //    Detector1.DisableGridVisibility = Detector1.Enabled ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
                        //    Detector1.BorderColorBrush = Detector1.Enabled ? new SolidColorBrush(Detector1.BorderColor) : new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
                        //    Detector2.ToggleSwitchEnabled = true;
                        //    Detector2.DisableGridVisibility = Detector2.Enabled ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
                        //    Detector2.BorderColorBrush = Detector2.Enabled ? new SolidColorBrush(Detector2.BorderColor) : new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
                        //    Detector3.ToggleSwitchEnabled = true;
                        //    Detector3.DisableGridVisibility = Detector3.Enabled ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
                        //    Detector3.BorderColorBrush = Detector3.Enabled ? new SolidColorBrush(Detector3.BorderColor) : new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
                        //    Detector4.ToggleSwitchEnabled = true;
                        //    Detector4.DisableGridVisibility = Detector4.Enabled ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
                        //    Detector4.BorderColorBrush = Detector4.Enabled ? new SolidColorBrush(Detector4.BorderColor) : new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
                        //    Detector5.ToggleSwitchEnabled = true;
                        //    Detector5.DisableGridVisibility = Detector5.Enabled ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
                        //    Detector5.BorderColorBrush = Detector5.Enabled ? new SolidColorBrush(Detector5.BorderColor) : new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));
                        //    Detector6.ToggleSwitchEnabled = true;
                        //    Detector6.DisableGridVisibility = Detector6.Enabled ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
                        //    Detector6.BorderColorBrush = Detector6.Enabled ? new SolidColorBrush(Detector6.BorderColor) : new SolidColorBrush(Color.FromArgb(255, 234, 234, 234));

                        //}

                        break; 
                    }
            }
        };

        _shellViewModel.PointClicked += OnMapPointClicked;
    }

    public void OnGridSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Any()) 
        {
            MeteoData selectedData = (MeteoData)e.AddedItems.First();
            _shellViewModel.PointToScale = selectedData.Id;
        } 
    }

    private void OnHistogramPointClicked(int id)
    {
        _shellViewModel.PointToScale = id;
    }

    private void OnMapPointClicked(MeteoData meteoData)
    {
        Histogram.ShowPointInfo(meteoData.Id);
    }
}
