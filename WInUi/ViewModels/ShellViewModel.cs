using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Navigation;
using URE.Services.Equipment;
using URE.Contracts.Services;
using URE.Core.Models.Meteo;
using URE.Core.Contracts.Repositories;
using URE.Core.Models.Equipment;
using Microsoft.UI.Xaml;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using URE.Core.Repositories;
using Microsoft.UI.Xaml.Controls;
using URE.Views;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;
using System.Drawing;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System.Media;
using Windows.Media.Playback;
using Windows.Media.Core;
using URE.Contracts.Services;
using URE.Core.Contracts.Services;
using Microsoft.AspNetCore.Identity;
using URE.Core.Models.Identity;
using System.Collections;
using URE.Extensions;
using Microsoft.Extensions.Configuration;
using URE.Core.Models;
using Microsoft.UI.Xaml.Data;
using Microsoft.Extensions.Options;

namespace URE.ViewModels;

public class ShellViewModel : ObservableRecipient, INotifyDataErrorInfo
{
    private bool _isBackEnabled;
    private object? _selected;
    private bool _renderHistoryClicked;
    private int _pointToScale;
    private object? currentContent;

    private List<MeteoData> _calendarDataCache = new List<MeteoData>();

    private MeteoData? _meteoData;
    public MeteoData MeteoData
    {
        get { return _meteoData; }
        set
        {
            _meteoData = value;
            OnPropertyChanged(nameof(MeteoData));
        }
    }

    public ObservableCollection<MeteoData> AutoStreamMeteoDatas { get; } = new();
    public ObservableCollection<MeteoData> ManualStreamMeteoDatas { get; } = new();
    private bool? isAutoStream = null;
    public bool? IsAutoStream { get => isAutoStream; set { SetProperty(ref isAutoStream, value); } }

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    public IMeteoStreamService MeteoStreamService
    {
        get;
    }

    public IMeteoStreamRepository MeteoStreamRepository
    {
        get;
    }

    public IMeteoDataRepository MeteoDataRepository
    {
        get;
    }

    public AppSettings AppSettings { get; }

    public EquipmentCollection Equipments { get; } 

    public IAlarmService AlarmService { get; }

    public ISignInManager SignInManager { get; }

    public UserManager<ApplicationUser> UserManager { get; }

    public RoleManager<ApplicationRole> RoleManager { get; }

    public GMSettings GMSettings
    {
        get;
    }

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public bool RenderHistoryClicked
    {
        get => _renderHistoryClicked;
        set
        {
            _renderHistoryClicked = value;
            OnPropertyChanged(nameof(RenderHistoryClicked));
        }
    }

    public int PointToScale
    {
        get => _pointToScale;
        set
        {
            _pointToScale = value;
            OnPropertyChanged(nameof(PointToScale));
        }
    }

    private string currentUserInfo;
    public string CurrentUserInfo { get => currentUserInfo; set => SetProperty(ref currentUserInfo, value); }

    private string speedMoveStr;
    public string SpeedMoveStr { get => speedMoveStr; 
        set 
        {
            if (!MoveSpeedEnabled) return;
            SetProperty(ref speedMoveStr, value); 
        } 
    }

    private string dateTimeStr;
    public string DateTimeStr
    {
        get => dateTimeStr;
        set
        {
            SetProperty(ref dateTimeStr, value);
        }
    }

    private double windDirection;
    public double WindDirection { get => windDirection; set => SetProperty(ref windDirection, value); }

    private double windSpeed;
    public double WindSpeed { get => windSpeed; set => SetProperty(ref windSpeed, value); }

    private string gpsHeightStr;
    public string GpsHeightStr { get => gpsHeightStr; set { if (!GpsHeightEnabled) return; SetProperty(ref gpsHeightStr, value); } }

    private string humidityStr;
    public string HumidityStr { get => humidityStr; set { if (!HumidityEnabled) return; SetProperty(ref humidityStr, value); } }

    private string preassureStr;
    public string PreassureStr { get => preassureStr; set { if (!PreassureEnabled) return; SetProperty(ref preassureStr, value); } }

    private string temperatureStr;
    public string TemperatureStr { get => temperatureStr; set { if (!TemperatureEnabled) return; SetProperty(ref temperatureStr, value); } }

    public bool IsSoundAlarmOn { get; set; } = true;

    private SolidColorBrush soundAlarmBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
    public SolidColorBrush SoundAlarmBtnBrush { get => soundAlarmBtnBrush; set => SetProperty(ref soundAlarmBtnBrush, value); }

    private SolidColorBrush lightAlarmBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
    public SolidColorBrush LightAlarmBtnBrush { get => lightAlarmBtnBrush; set => SetProperty(ref lightAlarmBtnBrush, value); }

    private SolidColorBrush gpsConnectedStateBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
    public SolidColorBrush GpsConnectedStateBtnBrush { get => gpsConnectedStateBtnBrush; set => SetProperty(ref gpsConnectedStateBtnBrush, value); }

    private SolidColorBrush gm0ConnectedStateBtnBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 108, 203, 95));
    public SolidColorBrush Gm0ConnectedStateBtnBrush { get => gm0ConnectedStateBtnBrush; set => SetProperty(ref gm0ConnectedStateBtnBrush, value); }

    private Visibility globalPreloaderVisibility = Visibility.Collapsed;
    public Visibility GlobalPreloaderVisibility { get => globalPreloaderVisibility; set => SetProperty(ref globalPreloaderVisibility, value); }

    private string soundStateIcon = "\xe767";
    public string SoundStateIcon { get => soundStateIcon; set => SetProperty(ref soundStateIcon, value); }

    private double maxDoseRate;
    public double MaxDoseRate { get => maxDoseRate;
        set
        {
            SetProperty(ref maxDoseRate, value);
        }
    }

    private Visibility normalDoseRateIconVisibility = Visibility.Visible;
    public Visibility NormalDoseRateIconVisibility { get => normalDoseRateIconVisibility; set { SetProperty(ref normalDoseRateIconVisibility, value); } }

    private Visibility highDoseRateIconVisibility = Visibility.Collapsed;
    public Visibility HighDoseRateIconVisibility { get => highDoseRateIconVisibility; set { SetProperty(ref highDoseRateIconVisibility, value); } }

    private Visibility criticalDoseRateIconVisibility = Visibility.Collapsed;
    public Visibility CriticalDoseRateIconVisibility { get => criticalDoseRateIconVisibility; set { SetProperty(ref criticalDoseRateIconVisibility, value); } }

    private Visibility calendarGridVisibility = Visibility.Collapsed;
    public Visibility CalendarGridVisibility { get => calendarGridVisibility; set { SetProperty(ref calendarGridVisibility, value); } }

    public ICommand CalendarGridVisibleCommand => new RelayCommand(() => { CalendarGridVisibility = Visibility.Visible; });
    public ICommand CalendarGridCollapsedCommand => new RelayCommand(() => { CalendarGridVisibility = Visibility.Collapsed; });

    private Visibility manualInputGridVisibility = Visibility.Collapsed;
    public Visibility ManualInputGridVisibility { get => manualInputGridVisibility; set { SetProperty(ref manualInputGridVisibility, value); } }

    public DateTime ManualInputMaxDateTime { get; private set; }

    public ICommand ManualInputGridVisibleCommand => new RelayCommand(() => { ManualInputMaxDateTime = DateTime.Now; ManualInputGridVisibility = Visibility.Visible; });
    public ICommand ManualInputGridCollapsedCommand => new RelayCommand(() => { ManualInputGridVisibility = Visibility.Collapsed; ClearErrors(); });

    private bool moveSpeedEnabled = true;
    public bool MoveSpeedEnabled { get => moveSpeedEnabled; 
        set 
        {
            if (!value) SpeedMoveStr = "0 км/год";
            SetProperty(ref moveSpeedEnabled, value); 
        } 
    }

    private bool moveSpeedIsActive;
    public bool MoveSpeedIsActive { get => moveSpeedIsActive; 
        set 
        {
            if (value)
            {
                SpeedMoveNoActiveVisibility = Visibility.Collapsed;
                SpeedMoveToggleSwitchVisibility = Visibility.Visible;
            }
            else
            {
                SpeedMoveNoActiveVisibility = Visibility.Visible;
                SpeedMoveToggleSwitchVisibility = Visibility.Collapsed;
                MoveSpeedEnabled = false;
            }

            SetProperty(ref moveSpeedIsActive, value);
        } 
    }

    private Visibility speedMoveToggleSwitchVisibility = Visibility.Collapsed;
    public Visibility SpeedMoveToggleSwitchVisibility { get => speedMoveToggleSwitchVisibility; set { SetProperty(ref speedMoveToggleSwitchVisibility, value); } }

    private Visibility speedMoveNoActiveVisibility = Visibility.Collapsed;
    public Visibility SpeedMoveNoActiveVisibility { get => speedMoveNoActiveVisibility; set { SetProperty(ref speedMoveNoActiveVisibility, value); } }

    private bool humidityEnabled = true;
    public bool HumidityEnabled
    {
        get => humidityEnabled;
        set
        {
            if (!value)
            {
                HumidityStr = "0 %";
                HumidityNoActiveVisibility = Visibility.Visible;
            }
            else HumidityNoActiveVisibility = Visibility.Collapsed;

            SetProperty(ref humidityEnabled, value);
        }
    }

    private Visibility humidityNoActiveVisibility = Visibility.Collapsed;
    public Visibility HumidityNoActiveVisibility { get => humidityNoActiveVisibility; set { SetProperty(ref humidityNoActiveVisibility, value); } }

    private bool temperatureEnabled = true;
    public bool TemperatureEnabled
    {
        get => temperatureEnabled;
        set
        {
            if (!value)
            {
                TemperatureStr = "0°C";
                TemperatureNoActiveVisibility = Visibility.Visible;
            }
            else TemperatureNoActiveVisibility = Visibility.Collapsed;

            SetProperty(ref temperatureEnabled, value);
        }
    }

    private Visibility temperatureNoActiveVisibility = Visibility.Collapsed;
    public Visibility TemperatureNoActiveVisibility { get => temperatureNoActiveVisibility; set { SetProperty(ref temperatureNoActiveVisibility, value); } }
   
    private bool preassureEnabled = true;
    public bool PreassureEnabled
    {
        get => preassureEnabled;
        set
        {
            if (!value)
            {
                PreassureStr = "0 гПа";
                PreassureNoActiveVisibility = Visibility.Visible;
            }
            else PreassureNoActiveVisibility = Visibility.Collapsed;

            SetProperty(ref preassureEnabled, value);
        }
    }

    private Visibility preassureNoActiveVisibility = Visibility.Collapsed;
    public Visibility PreassureNoActiveVisibility { get => preassureNoActiveVisibility; set { SetProperty(ref preassureNoActiveVisibility, value); } }

    private bool gpsHeightEnabled = true;
    public bool GpsHeightEnabled
    {
        get => gpsHeightEnabled;
        set
        {
            if (!value)
            {
                GpsHeightStr = "0 м";
                GpsHeightNoActiveVisibility = Visibility.Visible;
            }
            else GpsHeightNoActiveVisibility = Visibility.Collapsed;

            SetProperty(ref gpsHeightEnabled, value);
        }
    }

    private Visibility gpsHeightNoActiveVisibility = Visibility.Collapsed;
    public Visibility GpsHeightNoActiveVisibility { get => gpsHeightNoActiveVisibility; set { SetProperty(ref gpsHeightNoActiveVisibility, value); } }

    private bool windSpeedEnabled = true;
    public bool WindSpeedEnabled
    {
        get => windSpeedEnabled;
        set
        {
            SetProperty(ref windSpeedEnabled, value);
        }
    }

    private bool windDirectionEnabled = true;
    public bool WindDirectionEnabled
    {
        get => windDirectionEnabled;
        set
        {
            SetProperty(ref windDirectionEnabled, value);
        }
    }

    private Microsoft.UI.Xaml.Media.Brush manualInputDetectorBtnBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.White);
    public Microsoft.UI.Xaml.Media.Brush ManualInputDetectorBtnBrush { get => manualInputDetectorBtnBrush; set => SetProperty(ref manualInputDetectorBtnBrush, value); }

    private string manualInputDetectorBtnIcon = "/Assets/HandIcon.png";
    public string ManualInputDetectorBtnIcon { get => manualInputDetectorBtnIcon; set => SetProperty(ref manualInputDetectorBtnIcon, value); }

    private DateTimeOffset startOrEndManualStreamDate = DateTimeOffset.Now;
    public DateTimeOffset StartOrEndManualStreamDate { get => startOrEndManualStreamDate; set => SetProperty(ref startOrEndManualStreamDate, value); }
    private TimeSpan startOrEndManualStreamTime = DateTime.Now.TimeOfDay;
    public TimeSpan StartOrEndManualStreamTime { get => startOrEndManualStreamTime; set => SetProperty(ref startOrEndManualStreamTime, value); }

    private DateTimeOffset newManualRecordDate = DateTimeOffset.Now;
    public DateTimeOffset NewManualRecordDate { get => newManualRecordDate; set => SetProperty(ref newManualRecordDate, value); }
    private TimeSpan newManualRecordTime = DateTime.Now.TimeOfDay;
    public TimeSpan NewManualRecordTime { get => newManualRecordTime; set => SetProperty(ref newManualRecordTime, value); }
    private double manualDoseRateInput;
    public double ManualDoseRateInput { get => manualDoseRateInput; set { ValidateDoseRateManualInput(value); SetProperty(ref manualDoseRateInput, value); OnPropertyChanged(); } }
    public double NewManualRecordLon { get; set; }
    public double NewManualRecordLat { get; set; }

    private readonly IGPSSettingsRepository _gpsSettingsRepository;
    private readonly IMeteoSettingsRepository _meteoSettingsRepository;

    private readonly DispatcherQueue _dispatcherQueue;

    public event Action<MeteoData>? PointClicked;

    public ShellViewModel(INavigationService navigationService,
                          INavigationViewService navigationViewService,
                          IMeteoStreamService meteoStreamService,
                          IMeteoStreamRepository meteoStreamRepository,
                          IMeteoDataRepository meteoDataRepository,
                          IGMSettingsRepository gmSettingsRepository,
                          IGPSSettingsRepository gpsSettingsRepository,
                          IMeteoSettingsRepository meteoSettingsRepository,
                          IAlarmService soundAlarmService,
                          ISignInManager signInManager,
                          UserManager<ApplicationUser> userManager,
                          RoleManager<ApplicationRole> roleManager,
                          EquipmentCollection equipments,
                          IOptions<AppSettings> appSettings)
    {
        Equipments = equipments;
        RoleManager = roleManager;
        SignInManager = signInManager;
        UserManager = userManager;
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
        AppSettings = appSettings.Value;

        MeteoStreamService = meteoStreamService;
        MeteoStreamService.OnDataChanged += OnMeteoDataChanged;
        MeteoStreamService.OnDataStreamingStarted += OnMeteoDataStreamingStarted;
        MeteoStreamService.OnDataStreamingCompleted += OnMeteoDataStreamingCompleted;
        AlarmService = soundAlarmService;

        MeteoStreamService.StartListening();

        MeteoStreamRepository = meteoStreamRepository;
        MeteoDataRepository = meteoDataRepository;

        GMSettings = gmSettingsRepository.GetGMSettings();
        _gpsSettingsRepository = gpsSettingsRepository;
        _meteoSettingsRepository = meteoSettingsRepository;

        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        Task.Run(async () => {
            while (true)
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    DateTimeStr = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
                });
                await Task.Delay(1000);
            }
        });
    }

    public List<DateTimeOffset> SelectedDates { get; } = new List<DateTimeOffset>();

    public List<MeteoStream> Streams { get; set; } = new List<MeteoStream>();

    public int StreamsCountOnPage { get; } = 5;
    public int TotalPagesCount { get; set; } = 0;

    public IAsyncRelayCommand ToHistoryPageCommand => new AsyncRelayCommand(async () =>
    {
        var dates = SelectedDates.Select(x => x.Date).OrderByDescending(x => x).ToList();

        TotalPagesCount = (int)Math.Ceiling(await MeteoStreamRepository.GetStreamsCountByDates(dates) / (decimal)StreamsCountOnPage);

        NavigationService.NavigateTo(typeof(HistoryVm).FullName, new List<MeteoStream>(Streams));
        CalendarGridCollapsedCommand.Execute(null);
    });

    public List<MeteoStream> ImportedStreams { get; set; } = new();

    public async Task LoadStreamsPage(int pageIndex = 0, bool isImported = false)
    {
        GlobalPreloaderVisibility = Visibility.Visible;

        if (isImported)
        {
            Streams = ImportedStreams.OrderBy(x => x.DateStart).Skip(pageIndex * StreamsCountOnPage).Take(StreamsCountOnPage).ToList();
        }
        else
        {
            var dates = SelectedDates.Select(x => x.Date).OrderByDescending(x => x).ToList();

            Streams = await MeteoStreamRepository.GetByDates(dates, StreamsCountOnPage, pageIndex * StreamsCountOnPage);
        }

        RenderHistoryClicked = true;
        GlobalPreloaderVisibility = Visibility.Collapsed;
    }

    public async Task InitializeCalendarData(DateTime startDate, DateTime endDate)
    {
        _calendarDataCache.Clear();
        _calendarDataCache.AddRange(await MeteoDataRepository.GetByPeriod(startDate, endDate));
    }

    public void OnPointClicked(MeteoData data)
    {
        PointClicked?.Invoke(data);
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;
        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }

        var frame = sender as Frame;
        if (frame != null)
        {
            currentContent = frame.Content;
            ExecuteNavigatedPageCallback();
        }

        var gpsSettings = _gpsSettingsRepository.GetGPSSettingsOrNull();
        MoveSpeedIsActive = gpsSettings.MoveSpeedEnabled;
        GpsHeightEnabled = gpsSettings.HeightEnabled;

        var meteoSettings = _meteoSettingsRepository.GetMeteoSettingsOrNull();
        TemperatureEnabled = meteoSettings.TemperatureEnabled;
        HumidityEnabled = meteoSettings.HumidityEnabled;
        PreassureEnabled = meteoSettings.PreassureEnabled;
        WindDirectionEnabled = meteoSettings.WindDirectionEnabled;
        WindSpeedEnabled = meteoSettings.WindSpeedEnabled;
    }

    private void ExecuteNavigatedPageCallback()
    {
        var dataContextObj = currentContent?.GetType()?.GetProperty("DataContext")?.GetValue(currentContent);
        var dataContextType = dataContextObj?.GetType();

        if (dataContextType != null)
        {
            var handlerMethod = dataContextType.GetMethod("OnNavigated");
            handlerMethod?.Invoke(dataContextObj, new object[] { this }); ;
        }
    }

    private void OnMeteoDataStreamingStarted(bool auto)
    {
        IsAutoStream = auto;
        AutoStreamMeteoDatas.Clear();
        ManualStreamMeteoDatas.Clear();

        if (IsAutoStream.Value)
        {
            MeteoStreamService.OnDataChanged += OnMeteoDataChangedByStream;
        }
        else
        {
            MeteoStreamService.OnInputDataChanged += OnMeteoDataChangedByInputStream;
        }
    }

    private void OnMeteoDataStreamingCompleted()
    {
        if (IsAutoStream.HasValue)
        {
            if (IsAutoStream.Value)
            {
                MeteoStreamService.OnDataChanged -= OnMeteoDataChangedByStream;
            }
            else
            {
                MeteoStreamService.OnInputDataChanged -= OnMeteoDataChangedByInputStream;
            }

            IsAutoStream = null;
        }
    }

    private void OnMeteoDataChangedByStream(MeteoData meteoData)
    {
        MeteoData = meteoData;
        AutoStreamMeteoDatas.Add(meteoData);

        if(AutoStreamMeteoDatas.Count == 1) NavigationService.NavigateTo(typeof(MainViewModel).FullName);
    }

    private void OnMeteoDataChangedByInputStream(MeteoData meteoData)
    {
        MeteoData = meteoData;
        ManualStreamMeteoDatas.Add(meteoData);

        if (ManualStreamMeteoDatas.Count == 1) NavigationService.NavigateTo(typeof(MainViewModel).FullName);
    }

    private void OnMeteoDataChanged(MeteoData data)
    {
        //MeteoData = data;
        SpeedMoveStr = $"{Math.Round(data.Speed)} км/год";
        GpsHeightStr = $"{Math.Round(data.GPSHeight)} м";
        HumidityStr = $"{Math.Round(data.RelativeHumidity)} %";
        TemperatureStr = $"{Math.Round(data.Temperature)}°C";
        PreassureStr = $"{data.Pressure.ToString("0.0")} гПа";
        MaxDoseRate = data.MaxDoseRate;
        WindDirection = data.CorrectedDirection;
        WindSpeed = data.CorrectedSpeed;

        if (data != null)
        {
            var valueState = GMSettings.GetValueState(data);

            switch (valueState)
            {
                case SettingsValueState.Normal:
                    {
                        NormalDoseRateIconVisibility = Visibility.Visible;
                        HighDoseRateIconVisibility = Visibility.Collapsed;
                        CriticalDoseRateIconVisibility = Visibility.Collapsed;

                        break;
                    }
                case SettingsValueState.High:
                    {
                        NormalDoseRateIconVisibility = Visibility.Collapsed;
                        HighDoseRateIconVisibility = Visibility.Visible;
                        CriticalDoseRateIconVisibility = Visibility.Collapsed;

                        break;
                    }
                case SettingsValueState.Critical:
                    {
                        NormalDoseRateIconVisibility = Visibility.Collapsed;
                        HighDoseRateIconVisibility = Visibility.Collapsed;
                        CriticalDoseRateIconVisibility = Visibility.Visible;

                        break;
                    }
            }

            AlarmService.UpdatePlayingDoseRateAlarm(valueState);
        }
    }

    public IEnumerable<DateTime> GetMonthDatesByHighDoseRate(DateTime destDate)
    {
        DateTime firstDayMonth = new DateTime(destDate.Year, destDate.Month, 1).Date;
        DateTime lastDayMonth = firstDayMonth.AddMonths(1).AddDays(-1).Date;

        IEnumerable<MeteoData> meteoData = _calendarDataCache.Where(ms => ms.Date >= firstDayMonth && ms.Date <= lastDayMonth);
        return meteoData.Where(x => x.MaxDoseRate > GMSettings.HighValue).Select(x => x.Date);
    }

    private readonly Dictionary<string, ICollection<string>> _validationErrors = new();

    public bool HasErrors => _validationErrors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    public IEnumerable GetErrors(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName) ||
            !_validationErrors.ContainsKey(propertyName))
            return null;

        return _validationErrors[propertyName];
    }

    private void SetErrors(string key, ICollection<string> errors)
    {
        if (errors.Any())
            _validationErrors[key] = errors;
        else
            _ = _validationErrors.Remove(key);

        OnErrorsChanged(key);
    }

    public void ClearErrors()
    {
        foreach(var key in _validationErrors.Keys)
        {
            SetErrors(key, new List<string>());
        }
    }

    public bool ValidateDoseRateManualInput(double doseRate)
    {
        var errors = new List<string>(1);
        if (doseRate <= 0 || doseRate > 20000)
        {
            errors.Add("Показник має бути від 0,001 до 20000,000!");
        }

        SetErrors(nameof(ManualDoseRateInput), errors);

        return !HasErrors;
    }
}