using Microsoft.UI;
using System.ComponentModel;
using Mapsui.Tiling;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using Windows.UI;
using Windows.Foundation;
using URE.Contracts.Services;
using URE.Helpers;
using URE.ViewModels;
using URE.Core.Models.Equipment;
using URE.Core.Models.Meteo;
using URE.Core.Repositories;
using System.Management;
using Mapsui.UI.WinUI.Extensions;
using Mapsui;
using Mapsui.Projections;
using Mapsui.Nts.Extensions;
using Mapsui.UI.WinUI;
using LiveChartsCore.Geo;
using Mapsui.Extensions;
using CommunityToolkit.WinUI.UI;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Data.Text;
using URE.Core.Models.Identity;
using Mapsui.Nts;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Configuration;

namespace URE.Views;

// TODO: Update NavigationViewItem titles and icons in ShellPage.xaml.
public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel
    {
        get;
    }

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();

        ViewModel.PropertyChanged += OnPropertyChanged;

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        var mapControl = Map.GetMap();
        mapControl.Info += MapInfo;

        // TODO: Set the title bar icon by updating /Assets/WindowIcon.ico.
        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
    }

    public void OnAutoStreamButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsAutoStream == false) return; 

        var button = sender as Button;
        var grid = button?.Content as Grid;
        var textBlock = (grid?.Children?.FirstOrDefault() as StackPanel)?.Children?.FirstOrDefault() as TextBlock;
        var fontIcon = (grid?.Children?.FirstOrDefault() as StackPanel)?.Children?.LastOrDefault() as FontIcon;

        if (ViewModel.IsAutoStream == null)
        {
            Map.ClearMap();
            ViewModel.MeteoStreamService.StartStreamingAsync();
            //_streamStarted = true;
            //ViewModel.IsAutoStream = true;
            grid.Background = new SolidColorBrush(Color.FromArgb(255, 250, 181, 3));
            textBlock.Text = "StopBtnText".GetLocalized();
            fontIcon.Glyph = "\xe71a";

            ViewModel.ManualInputDetectorBtnBrush = new SolidColorBrush(Colors.DarkGray);
            ViewModel.ManualInputDetectorBtnIcon = "/Assets/WhiteHandIcon.png";

            //var mapc = (Map.Content as Grid).Children.First() as MapControl;
            //mapc.DoubleTapped += MapTapped;
        }
        else
        {
            ViewModel.MeteoStreamService.StopStreamingAsync();
            //_streamStarted = false;
            //ViewModel.IsAutoStream = null;
            grid.Background = new SolidColorBrush(Color.FromArgb(255, 0, 95, 184));
            textBlock.Text = "StartBtnText".GetLocalized();
            //Map.ClearMap();
            fontIcon.Glyph = "\xe768";

            ViewModel.ManualInputDetectorBtnBrush = new SolidColorBrush(Colors.White);
            ViewModel.ManualInputDetectorBtnIcon = "/Assets/HandIcon.png";
        }
    }


    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));

        //var mapc = (Map.Content as Grid).Children.First() as MapControl;
        //mapc.DoubleTappedTapped += (s,e) => {
        //    var mPoint = e.GetPosition(mapc).ToMapsui().ToCoordinate();

        //};

        if (!ViewModel.SignInManager.Identity.IsAuthenticated)
        {
            ViewModel.CurrentUserInfo = string.Empty;
            loginForm.Visibility = Visibility.Visible;
            signOutBtn.Visibility = Visibility.Collapsed;
        }
        else
        {
            ViewModel.CurrentUserInfo = ViewModel.SignInManager.Identity.User.PersonInfo;
            loginForm.Visibility = Visibility.Collapsed;
            signOutBtn.Visibility = Visibility.Visible;

            if (!ViewModel.SignInManager.Identity.Roles.Contains(Role.SuperAdmin))
            {
                equipmentSettingsNavItem.Visibility = Visibility.Collapsed;
            }

            if (!ViewModel.SignInManager.Identity.Roles.Contains(Role.Admin))
            {
                userSettingsNavItem.Visibility = Visibility.Collapsed;
            }
        }

        if (ViewModel.MoveSpeedEnabled && ViewModel.MoveSpeedIsActive)
        {
            speedMoveBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 108, 203, 95));
            moveSpeedDisabledGrid.Visibility = Visibility.Collapsed;
        }
        else
        {
            speedMoveBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
            moveSpeedDisabledGrid.Visibility = Visibility.Visible;
        }

        ViewModel.SignInManager.Login += (identity) =>
        {
            if (!ViewModel.SignInManager.Identity.Roles.Contains(Role.SuperAdmin))
            {
                equipmentSettingsNavItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                equipmentSettingsNavItem.Visibility = Visibility.Visible;
            }

            if (!ViewModel.SignInManager.Identity.Roles.Contains(Role.Admin))
            {
                userSettingsNavItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                userSettingsNavItem.Visibility = Visibility.Visible;
            }

            ViewModel.NavigationService.NavigateTo(typeof(MainViewModel).FullName);
        };

        ViewModel.Equipments[EquipmentType.GPS].OnConnectedChanged += () =>
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                if (ViewModel.Equipments[EquipmentType.GPS].IsConnected)
                {
                    ViewModel.GpsConnectedStateBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
                    ViewModel.LightAlarmBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
                }
                else
                {
                    ViewModel.GpsConnectedStateBtnBrush = new SolidColorBrush(Colors.Red);
                    ViewModel.LightAlarmBtnBrush = new SolidColorBrush(Colors.Red);
                    ViewModel.AlarmService.UpdatePlayingEquipmentAlarm(EquipmentType.GPS);
                }
            });
        };

        ViewModel.Equipments[EquipmentType.GM0].OnConnectedChanged += () =>
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                if (ViewModel.Equipments[EquipmentType.GM0].IsConnected)
                {
                    ViewModel.Gm0ConnectedStateBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
                    ViewModel.LightAlarmBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
                }
                else
                {
                    ViewModel.Gm0ConnectedStateBtnBrush = new SolidColorBrush(Colors.Red);
                    ViewModel.LightAlarmBtnBrush = new SolidColorBrush(Colors.Red);
                    ViewModel.AlarmService.UpdatePlayingEquipmentAlarm(EquipmentType.GM0);
                }
            });
        };

        if (!ViewModel.AppSettings.IsDevelopment)
        {
            if (ViewModel.Equipments[EquipmentType.GPS].IsConnected)
            {
                ViewModel.GpsConnectedStateBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
                ViewModel.LightAlarmBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
            }
            else
            {
                ViewModel.GpsConnectedStateBtnBrush = new SolidColorBrush(Colors.Red);
                ViewModel.LightAlarmBtnBrush = new SolidColorBrush(Colors.Red);
                ViewModel.AlarmService.UpdatePlayingEquipmentAlarm(EquipmentType.GPS);
            }

            if (ViewModel.Equipments[EquipmentType.GM0].IsConnected)
            {
                ViewModel.Gm0ConnectedStateBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
                ViewModel.LightAlarmBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
            }
            else
            {
                ViewModel.Gm0ConnectedStateBtnBrush = new SolidColorBrush(Colors.Red);
                ViewModel.LightAlarmBtnBrush = new SolidColorBrush(Colors.Red);
                ViewModel.AlarmService.UpdatePlayingEquipmentAlarm(EquipmentType.GM0);
            }
        }

        ViewModel.AlarmService.MuteAlarmStateChanged += () => {
            this.DispatcherQueue.TryEnqueue(() => {
                if (!ViewModel.AlarmService.IsAlarmMute)
                {
                    ViewModel.SoundStateIcon = "\xe767";

                    if (ViewModel.AlarmService.PlayingAlarmType == AlarmType.Equipment)
                    {
                        ViewModel.SoundAlarmBtnBrush = new SolidColorBrush(Colors.Red);
                    }
                    else if (ViewModel.AlarmService.DoseRateAlarmLvl == SettingsValueState.High)
                    {
                        ViewModel.SoundAlarmBtnBrush = new SolidColorBrush(Colors.Orange);
                    }
                    else if (ViewModel.AlarmService.DoseRateAlarmLvl == SettingsValueState.Critical)
                    {
                        ViewModel.SoundAlarmBtnBrush = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        ViewModel.SoundAlarmBtnBrush = new SolidColorBrush(Color.FromArgb(255, 108, 203, 95));
                    }
                }
                else
                {
                    ViewModel.SoundStateIcon = "\xe74f";
                    ViewModel.SoundAlarmBtnBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
                }
            });
        };

        ViewModel.AlarmService.PlayingAlarmUpdated += () =>
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                if (ViewModel.AppSettings.IsDevelopment || (ViewModel.Equipments[EquipmentType.GM0].IsConnected && ViewModel.Equipments[EquipmentType.GPS].IsConnected))
                {
                    if (ViewModel.AlarmService.DoseRateAlarmLvl == SettingsValueState.High)
                    {
                        ViewModel.LightAlarmBtnBrush = new SolidColorBrush(Colors.Orange);
                    }
                    else if (ViewModel.AlarmService.DoseRateAlarmLvl == SettingsValueState.Critical)
                    {
                        ViewModel.LightAlarmBtnBrush = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        ViewModel.LightAlarmBtnBrush = new SolidColorBrush(Color.FromArgb(255, 108, 203, 95));
                    }
                }

                if (!ViewModel.AlarmService.IsAlarmMute)
                {
                    if(ViewModel.AlarmService.PlayingAlarmType == AlarmType.Equipment)
                    {
                        ViewModel.SoundAlarmBtnBrush = new SolidColorBrush(Colors.Red);
                    }
                    else if(ViewModel.AlarmService.DoseRateAlarmLvl == SettingsValueState.High)
                    {
                        ViewModel.SoundAlarmBtnBrush = new SolidColorBrush(Colors.Orange);
                    }
                    else if (ViewModel.AlarmService.DoseRateAlarmLvl == SettingsValueState.Critical)
                    {
                        ViewModel.SoundAlarmBtnBrush = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        ViewModel.SoundAlarmBtnBrush = new SolidColorBrush(Color.FromArgb(255, 108, 203, 95));
                    }
                }
            });
        };
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        MeteoData meteoData = ViewModel.MeteoData;
        switch (e.PropertyName)
        {
            case nameof(ViewModel.MeteoData):
                SettingsValueState state = SettingsValueState.Low;

                if (ViewModel.IsAutoStream == true)
                    state = ViewModel.GMSettings.GetValueState(meteoData);
                else if (ViewModel.IsAutoStream == false)
                    state = ViewModel.GMSettings.GetValueState(meteoData.ManualInputRadiation);

                Map.AddPoint(meteoData.Id, meteoData.GPSLongitude, meteoData.GPSLatitude, state, true);

                break;
            case nameof(ViewModel.RenderHistoryClicked):
                if (ViewModel.RenderHistoryClicked) RenderStreamHistory();
                break;
            case nameof(ViewModel.PointToScale):
                ShowPointInfo(ViewModel.PointToScale, false, true, ViewModel.RenderHistoryClicked);
                break;
            default:
                break;

        }
    }

    private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsAutoStream != false)
        {
            if ((sender as ToggleSwitch)!.IsOn)
            {
                moveSpeedDisabledGrid.Visibility = Visibility.Collapsed;
                speedMoveBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 108, 203, 95));
                moveSpeedToolTipTextBlock.Text = "TooltipTextDetectorGraphOff".GetLocalized();
                //ViewModel.MoveSpeedEnabled = true;
            }
            else
            {
                moveSpeedDisabledGrid.Visibility = Visibility.Visible;
                speedMoveBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
                moveSpeedToolTipTextBlock.Text = "TooltipTextDetectorGraphOn".GetLocalized();
                //ViewModel.MoveSpeedEnabled = false;
            }
        }
    }

    private void CalendarView_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
    {
        ViewModel.SelectedDates.AddRange(args.AddedDates);
        ViewModel.SelectedDates.RemoveAll(x => args.RemovedDates.Contains(x));
    }

    private void RenderStreamHistory()
    {
        Map.ClearMap();
        List<(int Id, double Lon, double Lat, SettingsValueState ValueState)> points;

        foreach (var stream in ViewModel.Streams)
        {
            points = new List<(int Id, double Lon, double Lat, SettingsValueState ValueState)>(); 
            foreach (var data in stream.Data)
            {
                points.Add((data.Id, data.GPSLongitude, data.GPSLatitude, stream.Auto ? ViewModel.GMSettings.GetValueState(data) : ViewModel.GMSettings.GetValueState(data.ManualInputRadiation)));
            }

            Map.AddPoints(points);
        }
    }

    private void MapInfo(object sender, MapInfoEventArgs e)
    {
        MapControl map = sender as MapControl;

        if (e.MapInfo.Feature != null && e.MapInfo.Feature.Styles.FirstOrDefault() is Mapsui.Styles.SymbolStyle)
        {
            GeometryFeature pointFeature = e.MapInfo.Feature as GeometryFeature;
            int pointSourceId = int.Parse(pointFeature["SourceId"].ToString());
            bool isStartOrEnd = (bool)pointFeature["IsStartOrEnd"];
            var callout = e.MapInfo.MapInfoRecords.FirstOrDefault(r => r.Feature.Styles.FirstOrDefault() is Mapsui.Styles.CalloutStyle);
            if(callout != null)
            {
                GeometryFeature calloutFeature = (GeometryFeature)callout.Feature;
                int calloutSourceId = int.Parse(calloutFeature["SourceId"].ToString());
                
                if(calloutSourceId == pointSourceId)
                {
                    Map.HidePointInfo(calloutFeature);
                }
            }
            else
            {
                ShowPointInfo(pointSourceId, isStartOrEnd, history: ViewModel.RenderHistoryClicked, fromMap: true);
            }
        }
    }

    private void ShowPointInfo(int id, bool isStartOrEnd, bool scale = false, bool history = false, bool fromMap = false)
    {
        List<MeteoData> pointsData = new List<MeteoData>();

        if (history)
        {
            foreach (var streamData in ViewModel.Streams.Select(x => x.Data))
            {
                pointsData.AddRange(streamData);
            }
        }
        else
        {
            pointsData.AddRange(ViewModel.AutoStreamMeteoDatas);
            pointsData.AddRange(ViewModel.ManualStreamMeteoDatas);
        }

        MeteoData point = pointsData.FirstOrDefault(p => p.Id == id);

        if (point != null)
        {
            if (fromMap)
            {
                ViewModel.OnPointClicked(point);
            }

            if (!isStartOrEnd)
            {
                Map.ShowPointInfo(point.Id, point.GPSLongitude, point.GPSLatitude, point.MaxDoseRateStr, scale);
            }
            else
            {
                MeteoStream stream = ViewModel.Streams.FirstOrDefault(x => x.Id == point.MeteoStreamId);
                if (stream != null)
                {
                    Map.ShowStartOrEndPointInfo(point.Id, point.GPSLongitude, point.GPSLatitude, stream.User.PersonInfo, stream.DateStart, stream.DateEnd, scale);
                }
            }
        }
    }

    private void MapRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var map = sender as MapControl;
        var mPoint = e.GetPosition(map).ToMapsui();
        //var p = this.Map.CoordinatesFrom(this.Map);        
        //var s = SphericalMercator.FromLonLat(50.447100, 30.521876);//center of Kyiv
        //map.Map.Navigator.Viewport.CenterX
        var worldPoint = map.Map.Navigator.Viewport.ScreenToWorld(mPoint);
        var coordintaed = SphericalMercator.ToLonLat(worldPoint);

        ViewModel.NewManualRecordLon = coordintaed.X;
        ViewModel.NewManualRecordLat = coordintaed.Y;

        ViewModel.ManualInputGridVisibleCommand.Execute(null);
    }

    private async void OnManualNewRecordButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsAutoStream != false || !ViewModel.ValidateDoseRateManualInput(ViewModel.ManualDoseRateInput)) return;

        if (ViewModel.NewManualRecordDate.Date + ViewModel.NewManualRecordTime > ViewModel.ManualInputMaxDateTime)
        {
            manualInputErrorMsgTextBlock.Text = "Обрані дата та час не можуть бути більші за поточні!";
            return;
        }
        else
        {
            manualInputErrorMsgTextBlock.Text = string.Empty;
        }

        await ViewModel.MeteoStreamService.PushData(new MeteoData()
        {
            ManualInputRadiation = ViewModel.ManualDoseRateInput,
            Time = ViewModel.NewManualRecordTime,
            Date = ViewModel.NewManualRecordDate.Date,
            GPSLongitude = ViewModel.NewManualRecordLon,
            GPSLatitude = ViewModel.NewManualRecordLat
        });

        ViewModel.ManualInputGridCollapsedCommand.Execute(null);
    }

    private async void StartOrStopManualStreamButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsAutoStream == true) return;

        var mapc = Map.GetMap();

        if (ViewModel.IsAutoStream == null)
        {
            Map.ClearMap();

            await ViewModel.MeteoStreamService.StartStreamingAsync(true);

            mapc.RightTapped += MapRightTapped;
            //moveSpeedToggleSwitch.IsEnabled = false;
            //moveSpeedDisabledGrid.Visibility = Visibility.Visible;
            //windSpeedDisabledGrid.Visibility = Visibility.Visible;
            //windDirectionDisabledGrid.Visibility = Visibility.Visible;
            //temperatureDisabledGrid.Visibility = Visibility.Visible;
            //humidityDisabledGrid.Visibility = Visibility.Visible;
            //gpsHeightDisabledGrid.Visibility = Visibility.Visible;
            //speedMoveBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
            ViewModel.ManualInputDetectorBtnBrush = new SolidColorBrush(Color.FromArgb(255, 0, 95, 184));
            ViewModel.ManualInputDetectorBtnIcon = "/Assets/WhiteHandIcon.png";
            autoStreamBtnGrid.Background = new SolidColorBrush(Colors.DarkGray);
            //ViewModel.SpeedMoveStr = $"0 км/год";
            //ViewModel.GpsHeightStr = $"0 м";
            //ViewModel.HumidityStr = $"{/*Math.Round(data.RelativeHumidity)*/0} %";
            //ViewModel.TemperatureStr = $"{/*Math.Round(data.Temperature)*/0}°C";
            //ViewModel.WindSpeedStr = $"{/*Math.Round(data.CorrectedSpeed)*/0} м/с";
            //ViewModel.MaxDoseRate = 0;
            //ViewModel.IsAutoStream = false;
        }
        else
        {
            await ViewModel.MeteoStreamService.StopStreamingAsync(true);

            mapc.RightTapped -= MapRightTapped;

            ViewModel.ManualInputDetectorBtnBrush = new SolidColorBrush(Colors.White);
            ViewModel.ManualInputDetectorBtnIcon = "/Assets/HandIcon.png";
            autoStreamBtnGrid.Background = new SolidColorBrush(Color.FromArgb(255, 0, 95, 184));
            //moveSpeedToggleSwitch.IsEnabled = true;
            //windSpeedDisabledGrid.Visibility = Visibility.Collapsed;
            //windDirectionDisabledGrid.Visibility = Visibility.Collapsed;
            //temperatureDisabledGrid.Visibility = Visibility.Collapsed;
            //humidityDisabledGrid.Visibility = Visibility.Collapsed;
            //gpsHeightDisabledGrid.Visibility = Visibility.Collapsed;
            ViewModel.IsAutoStream = null;
            //if (ViewModel.MoveSpeedEnabled && ViewModel.MoveSpeedIsActive)
            //{
            //    speedMoveBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 108, 203, 95));
            //    //moveSpeedDisabledGrid.Visibility = Visibility.Collapsed;
            //}
        }

        //periodManualStreamInput.Visibility = Visibility.Collapsed;
    }

    private List<FrameworkElement> Children(DependencyObject parent)
    {
        var list = new List<FrameworkElement>();

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is FrameworkElement)
            {
                list.Add(child as FrameworkElement);
            }

            list.AddRange(Children(child));
        }

        return list;
    }

    private void HidePeriodManualStreamInputButton_Click(object sender, RoutedEventArgs e)
    {
        periodManualStreamInput.Visibility = Visibility.Collapsed;
    }

    private async void ShowCalendarButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.GlobalPreloaderVisibility = Visibility.Visible;
        ViewModel.SelectedDates.Clear();

        var newCalendar = new CalendarView();
        newCalendar.SelectionMode = CalendarViewSelectionMode.Single;
        newCalendar.CalendarViewDayItemChanging += CalendarView_CalendarViewDayItemChanging;
        newCalendar.SelectedDatesChanged += CalendarView_SelectedDatesChanged;
        newCalendar.SelectedBorderBrush = new SolidColorBrush(Colors.Black);
        newCalendar.FirstDayOfWeek = Windows.Globalization.DayOfWeek.Monday;
        newCalendar.TodayForeground = new SolidColorBrush(Colors.Black);
        newCalendar.IsTodayHighlighted = false;
        newCalendar.TodayFontWeight = new Windows.UI.Text.FontWeight(17);
        newCalendar.MaxDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

        DateTime minDate = DateTime.Now.AddMonths(-2);
        newCalendar.MinDate = new DateTime(minDate.Year, minDate.Month, 1);

        calendarBorder.Child = newCalendar;

        await ViewModel.InitializeCalendarData(newCalendar.MinDate.Date, newCalendar.MaxDate.Date);

        ViewModel.GlobalPreloaderVisibility = Visibility.Collapsed;

        ViewModel.CalendarGridVisibility = Visibility.Visible;
    }

    private void CalendarView_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
    {
        if (args.Item.Date.AddDays(1).Month != args.Item.Date.Month)
        {
            var highDoseRateDates = ViewModel.GetMonthDatesByHighDoseRate(args.Item.Date.Date);
            var days = Children(sender).OfType<CalendarViewDayItem>();

            foreach (var day in days)
            {
                if (highDoseRateDates.Contains(day.Date.Date))
                {
                    day.Background = new SolidColorBrush(Colors.MistyRose);
                }
            }
        }
    }

    private void PointHighDose(IEnumerable<CalendarViewDayItem> days, DateTime destDate)
    {
        var highDoseRateDates = ViewModel.GetMonthDatesByHighDoseRate(destDate);
        foreach (var day in days)
        {
            if (highDoseRateDates.Contains(day.Date.Date))
            {
                day.Background = new SolidColorBrush(Colors.MistyRose);
            }
        }
    }

    private void SoundAlarmOnOffButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.AlarmService.IsDoseRateAlarmMute && 
            ViewModel.AlarmService.IsMeteoAlarmMute && 
            ViewModel.AlarmService.IsGpsAlarmMute && 
            ViewModel.AlarmService.IsGm0AlarmMute)
        {
            ViewModel.AlarmService.UpdateMuteAlarm(false);
        }
        else
        {
            ViewModel.AlarmService.UpdateMuteAlarm(true);
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var user = await ViewModel.UserManager.Users.FirstOrDefaultAsync(x => x.UserName.Equals(loginTextBox.Text));

        if (user != null)
        {
            var result = await ViewModel.UserManager.CheckPasswordAsync(user, passwordTextBox.Password);

            if (result)
            {
                await ViewModel.SignInManager.SignInAsync(user);
                userNameTextBlock.Text = user.PersonInfo;
                loginForm.Visibility = Visibility.Collapsed;
                signOutBtn.Visibility = Visibility.Visible;
                loginErrorMsg.Text = string.Empty;
                loginTextBox.Text = string.Empty;
                passwordTextBox.Password = string.Empty;
            }
            else
            {
                loginErrorMsg.Text = "Вказано невірний пароль!";
            }
        }
        else
        {
            loginErrorMsg.Text = "Вказано невірний логін!";
        }
    }

    private async void SignOutButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SignInManager.SignOutAsync();
        userNameTextBlock.Text = string.Empty;
        loginForm.Visibility = Visibility.Visible;
        signOutBtn.Visibility = Visibility.Collapsed;
        signOutForm.Visibility = Visibility.Collapsed;
    }

    private void HideSignOutFormButton_Click(object sender, RoutedEventArgs e)
    {
        signOutForm.Visibility = Visibility.Collapsed;
    }

    private void ShowSignOutFormButton_Click(object sender, RoutedEventArgs e)
    {
        signOutForm.Visibility = Visibility.Visible;
    }

    private void GpsSoundAlarmMuteButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.AlarmService.IsGpsAlarmMute)
        {
            ViewModel.AlarmService.UpdateMuteEquipmentAlarm(true, EquipmentType.GPS);
            ViewModel.GpsConnectedStateBtnBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
            ViewModel.AlarmService.MuteAlarmStateChanged += SetSoundAlarmMuteVisual;
        }
        else
        {
            ViewModel.AlarmService.UpdateMuteEquipmentAlarm(false, EquipmentType.GPS);
            ViewModel.GpsConnectedStateBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
            ViewModel.AlarmService.MuteAlarmStateChanged -= SetSoundAlarmMuteVisual;
        }

        void SetSoundAlarmMuteVisual()
        {
            if (!ViewModel.AlarmService.IsGpsAlarmMute)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (ViewModel.AppSettings.IsDevelopment)
                    {
                        ViewModel.GpsConnectedStateBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
                    }
                    else if (ViewModel.Equipments[EquipmentType.GPS].IsConnected)
                    {
                        ViewModel.GpsConnectedStateBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
                    }
                    else
                    {
                        ViewModel.GpsConnectedStateBtnBrush = new SolidColorBrush(Colors.Red);
                    }
                });
            }
        }
    }

    private void Gm0SoundAlarmMuteButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.AlarmService.IsGm0AlarmMute)
        {
            ViewModel.AlarmService.UpdateMuteEquipmentAlarm(true, EquipmentType.GM0);
            ViewModel.Gm0ConnectedStateBtnBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
            ViewModel.AlarmService.MuteAlarmStateChanged += SetSoundAlarmMuteVisual;
        }
        else
        {
            ViewModel.AlarmService.UpdateMuteEquipmentAlarm(false, EquipmentType.GM0);
            ViewModel.Gm0ConnectedStateBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
            ViewModel.AlarmService.MuteAlarmStateChanged -= SetSoundAlarmMuteVisual;
        }

        void SetSoundAlarmMuteVisual()
        {
            if (!ViewModel.AlarmService.IsGm0AlarmMute)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (ViewModel.AppSettings.IsDevelopment)
                    {
                        ViewModel.Gm0ConnectedStateBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
                    }
                    else if (ViewModel.Equipments[EquipmentType.GM0].IsConnected)
                    {
                        ViewModel.Gm0ConnectedStateBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
                    }
                    else
                    {
                        ViewModel.Gm0ConnectedStateBtnBrush = new SolidColorBrush(Colors.Red);
                    }
                });
            }
        }
    }

    private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Escape:
                {
                    if (ViewModel.CalendarGridVisibility == Visibility.Visible)
                    {
                        ViewModel.CalendarGridCollapsedCommand.Execute(null);
                    }

                    break;
                }
        }
    }
}
