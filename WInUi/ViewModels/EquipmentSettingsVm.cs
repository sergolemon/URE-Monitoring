using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI;
using URE.ViewModels.Controls;
using URE.Extensions;
using URE.Core.Models.Equipment;
using Microsoft.UI.Xaml.Media;
using URE.Core.Models;
using URE.Core.Contracts.Repositories;
using Microsoft.UI;
using System.Diagnostics.Metrics;
using BruTile.Predefined;
using Mapsui.Projections;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using Mapsui;
using URE.Services;
using Mapsui.Extensions;
using BruTile.Web;
using Microsoft.UI.Dispatching;
using Windows.Networking.Connectivity;
using System.ComponentModel;
using System.Collections;
using System.Runtime.CompilerServices;
using System.IO.Ports;

namespace URE.ViewModels
{
    public class EquipmentSettingsVm : ObservableRecipient, INotifyDataErrorInfo
    {
        private readonly IGMSettingsRepository _settingsRepository;
        private readonly EquipmentCollection _equipment;
        private readonly AppSettings _appSettings;

        public EquipmentSettingsVm(IGMSettingsRepository settingsRepository,
                                   IGPSSettingsRepository gpsSettingsRepository,
                                   IMeteoSettingsRepository meteoSettingsRepository,
                                   EquipmentCollection equipment, 
                                   IOptions<AppSettings> appSettings)
        {
            _settingsRepository = settingsRepository;
            _equipment = equipment;
            _appSettings = appSettings.Value;

            var currSettings = _settingsRepository.GetGMSettings();
            var detectorsArr = new DetectorConfigVm[6];
            if (currSettings != null) 
            {
                NormalDoseRate = currSettings.NormalValue;
                HighDoseRate = currSettings.HighValue;
                CriticalDoseRate = currSettings.CriticalValue;
                ComPortConfig.PortName = currSettings.SerialPortName;
                ComPortConfig.BaudRate = currSettings.BaudRate;
                ComPortConfig.Id = currSettings.Id;
                SoundAlarmConfig.Duration = currSettings.Duration;
                SoundAlarmConfig.RepetitionInterval = currSettings.RepetitionInterval;
                SoundAlarmConfig.BlackoutPeriod = currSettings.BlackoutPeriod;

                for (int i = 0; i < 6; i++)
                {
                    var el = currSettings.DetectorSettings?.ElementAtOrDefault(i);

                    if (el == null) detectorsArr[i] = new();
                    else
                    {
                        detectorsArr[i] = new DetectorConfigVm
                        {
                            DetectorIdentifier = el.EquipmentIdentifier,
                            DetectorName = el.Name,
                            DetectorEnabled = el.IsEnabled,
                            DetectorSerialNumber = el.SerialNumber,
                            Id = el.Id,
                            PortId = el.GmSettingsId,
                            NormalDoseRate = el.NormalValue,
                            HighDoseRate= el.HighValue,
                            CriticalDoseRate= el.CriticalValue
                        };
                        detectorsArr[i].HistogramLineColor = ColorExtensions.FromInt(el.Color);
                        detectorsArr[i].OnSelectThisDetector += (detector) => { SelectedDetectorConfig = detector; };
                    }
                }
            }

            Detector1Config = detectorsArr[0];
            Detector2Config = detectorsArr[1];
            Detector3Config = detectorsArr[2];
            Detector4Config = detectorsArr[3];
            Detector5Config = detectorsArr[4];
            Detector6Config = detectorsArr[5];

            //PropertyChanged += (s, e) => { 
                
            //};

            Detector1Config.PropertyChanged += (s, e) => { WasChangedData = true; };
            Detector2Config.PropertyChanged += (s, e) => { WasChangedData = true; };
            Detector3Config.PropertyChanged += (s, e) => { WasChangedData = true; };
            Detector4Config.PropertyChanged += (s, e) => { WasChangedData = true; };
            Detector5Config.PropertyChanged += (s, e) => { WasChangedData = true; };
            Detector6Config.PropertyChanged += (s, e) => { WasChangedData = true; };
            ComPortConfig.PropertyChanged += (s, e) => { WasChangedData = true; };
            SoundAlarmConfig.PropertyChanged += (s, e) => { WasChangedData = true; };
            PropertyChanged += (s, e) => { if(!new string[] { nameof(WasChangedData), nameof(SaveButtonColorBrush), nameof(SelectedDetectorConfig), nameof(DetectorDetailsConfigGridVisible) }.Contains(e.PropertyName)) WasChangedData = true; };
            MeteoStationSettings = new(meteoSettingsRepository, equipment, appSettings);
            GpsSettings = new(gpsSettingsRepository, equipment, appSettings);

            WasChangedData = false;
        }

        private Brush saveButtonColorBrush;
        public Brush SaveButtonColorBrush { get => saveButtonColorBrush; set { SetProperty(ref saveButtonColorBrush, value); } }
        private bool wasChangedData;
        public bool WasChangedData { get => wasChangedData; 
            set 
            { 
                SetProperty(ref wasChangedData, value);
                if(value) SaveButtonColorBrush = new SolidColorBrush(Color.FromArgb(255, 0, 95, 184));
                else SaveButtonColorBrush = new SolidColorBrush(Color.FromArgb(255, 94,166,238));
            } 
        }

        private Visibility detectorDetailsConfigGridVisible = Visibility.Collapsed;
        public Visibility DetectorDetailsConfigGridVisible { get => detectorDetailsConfigGridVisible; set { SetProperty(ref detectorDetailsConfigGridVisible, value); } }

        private Visibility portConfigGridVisible = Visibility.Collapsed;
        public Visibility PortConfigGridVisible { get => portConfigGridVisible; set { SetProperty(ref portConfigGridVisible, value); } }

        private DetectorConfigVm selectedDetectorConfig;
        public DetectorConfigVm SelectedDetectorConfig { get => selectedDetectorConfig; set { SetProperty(ref selectedDetectorConfig, value); DetectorDetailsConfigGridVisible = Visibility.Visible; } }

        private SerialPortConfigVm comPortConfig = new();
        public SerialPortConfigVm ComPortConfig { get => comPortConfig; set { SetProperty(ref comPortConfig, value); } }

        private SoundAlarmConfigVm soundAlarmConfig = new();
        public SoundAlarmConfigVm SoundAlarmConfig { get => soundAlarmConfig; set { SetProperty(ref soundAlarmConfig, value); } }

        private static MapConfigVm mapConfig = new();
        public MapConfigVm MapConfig => mapConfig;

        public ICommand DetectorDetailsConfigGridHideCommand
        {
            get => new RelayCommand(() =>
            {
                if (!SelectedDetectorConfig.ValidateDetectorIdentifier(SelectedDetectorConfig.DetectorIdentifier)) return;
                DetectorDetailsConfigGridVisible = Visibility.Collapsed;
            });
        }

        public ICommand SaveChangesCommand
        {
            get => new RelayCommand(async () =>
            {
                if (!ValidateModel()) return;

                var detectorsArr = new DetectorConfigVm[] 
                { 
                    Detector1Config,
                    Detector2Config,
                    Detector3Config,
                    Detector4Config,
                    Detector5Config,
                    Detector6Config,
                };
                var newEntity = ShellViewModel.GMSettings;
                newEntity.Id = ComPortConfig.Id;
                newEntity.CriticalValue = CriticalDoseRate;
                newEntity.HighValue = HighDoseRate;
                newEntity.NormalValue = NormalDoseRate;
                newEntity.BaudRate = ComPortConfig.BaudRate;
                newEntity.SerialPortName = ComPortConfig.PortName;
                newEntity.DetectorSettings = detectorsArr.Select(x => new DetectorSettings 
                { 
                    SerialNumber = x.DetectorSerialNumber,
                    EquipmentIdentifier = x.DetectorIdentifier,
                    GmSettingsId = x.PortId,
                    Color = x.HistogramLineColor.ToInt(),
                    Id = x.Id,
                    IsEnabled = x.DetectorEnabled,
                    Name = x.DetectorName,
                    NormalValue = x.NormalDoseRate,
                    HighValue = x.HighDoseRate,
                    CriticalValue = x.CriticalDoseRate
                }).ToList();
                newEntity.BlackoutPeriod = SoundAlarmConfig.BlackoutPeriod;
                newEntity.Duration = SoundAlarmConfig.Duration;
                newEntity.RepetitionInterval = SoundAlarmConfig.RepetitionInterval;
              
                await _settingsRepository.AddOrUpdateGMSettingsAsync(newEntity);
                WasChangedData = false;

                if (!_appSettings.IsDevelopment)
                {
                    _equipment.Connect(EquipmentType.GM0, true);
                }
            });
        }

        private MeteoStationSettingsVm meteoStationSettings;
        public MeteoStationSettingsVm MeteoStationSettings { get => meteoStationSettings; set => SetProperty(ref meteoStationSettings, value); }

        private GpsSettingsVm gpsSettings;
        public GpsSettingsVm GpsSettings { get => gpsSettings; set => SetProperty(ref gpsSettings, value); }

        private DetectorConfigVm detector1Config;
        public DetectorConfigVm Detector1Config { get => detector1Config; set { SetProperty(ref detector1Config, value); } }
        private DetectorConfigVm detector2Config;
        public DetectorConfigVm Detector2Config { get => detector2Config; set { SetProperty(ref detector2Config, value); } }
        private DetectorConfigVm detector3Config;
        public DetectorConfigVm Detector3Config { get => detector3Config; set { SetProperty(ref detector3Config, value); } }
        private DetectorConfigVm detector4Config;
        public DetectorConfigVm Detector4Config { get => detector4Config; set { SetProperty(ref detector4Config, value); } }
        private DetectorConfigVm detector5Config;
        public DetectorConfigVm Detector5Config { get => detector5Config; set { SetProperty(ref detector5Config, value); } }
        private DetectorConfigVm detector6Config;
        public DetectorConfigVm Detector6Config { get => detector6Config; set { SetProperty(ref detector6Config, value); } }

        //пороговые показатели для отображения на UI
        private double normalDoseRate;
        public double NormalDoseRate { get => normalDoseRate; set { ValidateDoseRate(value); SetProperty(ref normalDoseRate, value); OnPropertyChanged(); } }
        private double highDoseRate;
        public double HighDoseRate { get => highDoseRate; set { ValidateDoseRate(value); SetProperty(ref highDoseRate, value); OnPropertyChanged(); NormalDoseRate = NormalDoseRate; } }
        private double criticalDoseRate;
        public double CriticalDoseRate { get => criticalDoseRate; set { ValidateDoseRate(value); SetProperty(ref criticalDoseRate, value); OnPropertyChanged(); HighDoseRate = HighDoseRate; } }

        public ShellViewModel ShellViewModel { get; private set; }

        public void OnNavigated(ShellViewModel shellViewModel)
        {
            ShellViewModel = shellViewModel;

            GpsSettings.PropertyChanged += (s, e) =>
            {
                if (shellViewModel != null && e.PropertyName.Equals(nameof(GpsSettings.MoveSpeedEnabled)))
                {
                    shellViewModel.MoveSpeedEnabled = GpsSettings.MoveSpeedEnabled;
                    shellViewModel.MoveSpeedIsActive = GpsSettings.MoveSpeedEnabled;
                }
                else if (e.PropertyName.Equals(nameof(GpsSettings.HeightEnabled)))
                {
                    shellViewModel.GpsHeightEnabled = GpsSettings.HeightEnabled;
                }
            };

            MeteoStationSettings.PropertyChanged += (s, e) =>
            {
                if (shellViewModel != null)
                {
                    if (e.PropertyName.Equals(nameof(MeteoStationSettings.HumidityEnabled)))
                    {
                        shellViewModel.HumidityEnabled = MeteoStationSettings.HumidityEnabled;
                    }
                    else if (e.PropertyName.Equals(nameof(MeteoStationSettings.TemperatureEnabled)))
                    {
                        shellViewModel.TemperatureEnabled = MeteoStationSettings.TemperatureEnabled;
                    }
                    else if (e.PropertyName.Equals(nameof(MeteoStationSettings.PreassureEnabled)))
                    {
                        shellViewModel.PreassureEnabled = MeteoStationSettings.PreassureEnabled;
                    }
                    else if (e.PropertyName.Equals(nameof(MeteoStationSettings.WindSpeedEnabled)))
                    {
                        shellViewModel.WindSpeedEnabled = MeteoStationSettings.WindSpeedEnabled;
                    }
                    else if (e.PropertyName.Equals(nameof(MeteoStationSettings.WindDirectionEnabled)))
                    {
                        shellViewModel.WindDirectionEnabled = MeteoStationSettings.WindDirectionEnabled;
                    }
                }
            };
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
            foreach (var key in _validationErrors.Keys)
            {
                SetErrors(key, new List<string>());
            }
        }

        public bool ValidateDoseRate(double doseRate, [CallerMemberName] string? propertyName = null)
        {
            var errors = new List<string>(1);
            if (doseRate < 0 || doseRate > 20000)
            {
                errors.Add("Показник має бути від 0,001 до 20000,000!");
            }
            else if (propertyName == nameof(NormalDoseRate) && doseRate >= HighDoseRate)
            {
                errors.Add("Нормальний показник має бути меншим за підвищений!");
            }
            else if (propertyName == nameof(HighDoseRate) && doseRate >= CriticalDoseRate)
            {
                errors.Add("Підвищений показник має бути меншим за критичний!");
            }

            SetErrors(propertyName, errors);

            return !HasErrors;
        }

        public bool ValidateModel()
        {
            return !new bool[]
            {
                ValidateDoseRate(NormalDoseRate, nameof(NormalDoseRate)),
                ValidateDoseRate(HighDoseRate, nameof(HighDoseRate)),
                ValidateDoseRate(CriticalDoseRate, nameof(CriticalDoseRate))
            }.Any(x => !x);
        }
    }

    public class GpsSettingsVm : ObservableRecipient, INotifyDataErrorInfo
    {
        private readonly IGPSSettingsRepository _gpsSettingsRepository;
        private readonly EquipmentCollection _equipment;
        private readonly AppSettings _appSettings;

        public GpsSettingsVm(IGPSSettingsRepository gpsSettingsRepository,
                                      EquipmentCollection equipment,
                                      IOptions<AppSettings> appSettings)
        {
            _gpsSettingsRepository = gpsSettingsRepository;
            _equipment = equipment;
            _appSettings = appSettings.Value;

            //MoveSpeedColor = Colors.Lime;

            var settings = gpsSettingsRepository.GetGPSSettingsOrNull();

            Id = settings?.Id ?? 0;
            PortName = settings?.SerialPortName!;
            BaudRate = settings?.BaudRate ?? 0;
            MinMoveSpeed = settings != null ? Convert.ToInt32(settings.MinMoveSpeed) : 0;
            MaxMoveSpeed = settings != null ? Convert.ToInt32(settings.MaxMoveSpeed) : 0;
            MoveSpeedEnabled = settings?.MoveSpeedEnabled ?? false;
            MoveSpeedColor = (settings?.MoveSpeedColor ?? 0) == 0 ? Colors.Lime : ColorExtensions.FromInt(settings.MoveSpeedColor);
            HeightEnabled = settings?.HeightEnabled ?? false;

            PropertyChanged += (s, e) => { if (!new string[] { nameof(WasChangedData), nameof(SaveButtonColorBrush) }.Contains(e.PropertyName)) WasChangedData = true; };
            WasChangedData = false;
        }

        private Color moveSpeedColor;
        public Color MoveSpeedColor { get => moveSpeedColor; set { SetProperty(ref moveSpeedColor, value); MoveSpeedColorBrush = new SolidColorBrush(value); } }
        private Brush moveSpeedColorBrush;
        public Brush MoveSpeedColorBrush { get => moveSpeedColorBrush; set => SetProperty(ref moveSpeedColorBrush, value); }


        public int Id { get; set; }

        private List<Tuple<string, string>> accessedPorts = new();
        public List<Tuple<string, string>> AccessedPorts { get => accessedPorts; set => SetProperty(ref accessedPorts, value); }

        public List<Tuple<string, int>> AccessedBoudRates => new List<Tuple<string, int>>()
        {
            Tuple.Create("9600", 9600),
            Tuple.Create("14400", 14400),
            Tuple.Create("19200", 19200),
            Tuple.Create("56000", 56000),
            Tuple.Create("57600", 57600),
            Tuple.Create("115200", 115200),
            Tuple.Create("128000", 128000),
            Tuple.Create("256000", 256000)
        };

        private string portName = SerialPort.GetPortNames().FirstOrDefault() ?? string.Empty;
        public string PortName { get => portName; set => SetProperty(ref portName, value); }

        private int baudRate = 9600;
        public int BaudRate { get => baudRate; set => SetProperty(ref baudRate, value); }

        //private bool heightEnabled;
        //public bool HeightEnabled
        //{
        //    get => heightEnabled;
        //    set
        //    {
        //        SetProperty(ref heightEnabled, value); SetMeteoStationEnabled(value);
        //    }
        //}
        private bool moveSpeedEnabled;
        public bool MoveSpeedEnabled
        {
            get => moveSpeedEnabled;
            set => SetProperty(ref moveSpeedEnabled, value);
        }

        private int minMoveSpeed;
        public int MinMoveSpeed { get => minMoveSpeed; set { ValidateMinMoveSpeed(value); SetProperty(ref minMoveSpeed, value); OnPropertyChanged(); } }

        private int maxMoveSpeed;
        public int MaxMoveSpeed { get => maxMoveSpeed; set { ValidateMaxMoveSpeed(value); SetProperty(ref maxMoveSpeed, value); OnPropertyChanged(); } }

        private bool heightEnabled;
        public bool HeightEnabled
        {
            get => heightEnabled;
            set
            {
                SetProperty(ref heightEnabled, value);
            }
        }

        private Brush saveButtonColorBrush;
        public Brush SaveButtonColorBrush { get => saveButtonColorBrush; set { SetProperty(ref saveButtonColorBrush, value); } }
        private bool wasChangedData;
        public bool WasChangedData
        {
            get => wasChangedData;
            set
            {
                SetProperty(ref wasChangedData, value);
                if (value) SaveButtonColorBrush = new SolidColorBrush(Color.FromArgb(255, 0, 95, 184));
                else SaveButtonColorBrush = new SolidColorBrush(Color.FromArgb(255, 94, 166, 238));
            }
        }

        public ICommand SaveChangesCommand => new RelayCommand(async () =>
        {
            if (!ValidateModel()) return;

            await _gpsSettingsRepository.AddOrUpdateGPSSettingsAsync(new GPSSettings
            {
                BaudRate = BaudRate,
                SerialPortName = PortName,
                Id = Id,
                MoveSpeedEnabled = MoveSpeedEnabled,
                MaxMoveSpeed = MaxMoveSpeed,
                MinMoveSpeed = MinMoveSpeed,
                MoveSpeedColor = MoveSpeedColor.ToInt(),
                HeightEnabled = HeightEnabled
            });

            if (!_appSettings.IsDevelopment)
            {
                _equipment.Connect(EquipmentType.GPS, true);
            }

            WasChangedData = false;
        });

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
            foreach (var key in _validationErrors.Keys)
            {
                SetErrors(key, new List<string>());
            }
        }

        public bool ValidateMinMoveSpeed(double moveSpeed)
        {
            var errors = new List<string>(1);
            if (moveSpeed < 0 || moveSpeed > 200)
            {
                errors.Add("Показник швидкості руху має бути від 0 до 200!");
            }

            SetErrors(nameof(MinMoveSpeed), errors);

            return !HasErrors;
        }

        public bool ValidateMaxMoveSpeed(double moveSpeed)
        {
            var errors = new List<string>(1);
            if (moveSpeed < 0 || moveSpeed > 200)
            {
                errors.Add("Показник швидкості руху має бути від 0 до 200!");
            }
            else if (moveSpeed <= MinMoveSpeed)
            {
                errors.Add("Показник максимальної швидкості руху має бути більшим за показник мінімальної швидкості руху!");
            }

            SetErrors(nameof(MaxMoveSpeed), errors);

            return !HasErrors;
        }

        public bool ValidateModel()
        {
            return !new bool[]
            {
                ValidateMaxMoveSpeed(MaxMoveSpeed),
                ValidateMinMoveSpeed(MinMoveSpeed)
            }.Any(x => !x);
        }
    }

    public class MeteoStationSettingsVm : ObservableRecipient, INotifyDataErrorInfo
    {
        private readonly IMeteoSettingsRepository _meteoSettingsRepository;
        private readonly EquipmentCollection _equipment;
        private readonly AppSettings _appSettings;

        public MeteoStationSettingsVm(IMeteoSettingsRepository meteoSettingsRepository,
                                      EquipmentCollection equipment,
                                      IOptions<AppSettings> appSettings)
        {
            _equipment = equipment;
            _appSettings = appSettings.Value;

            _meteoSettingsRepository = meteoSettingsRepository;
            var settings = meteoSettingsRepository.GetMeteoSettingsOrNull();

            Id = settings?.Id ?? 0;
            EquipmentSerialNumber = settings?.SerialNumber ?? 0;
            EquipmentIdentifier = settings?.EquipmentIdentifier ?? 0;
            PortName = settings?.SerialPortName!;
            BaudRate = settings?.BaudRate ?? 0;
            WindDirectionEnabled = settings?.WindDirectionEnabled ?? false;
            HumidityEnabled = settings?.HumidityEnabled ?? false;
            TemperatureEnabled = settings?.TemperatureEnabled ?? false;
            WindSpeedEnabled = settings?.WindSpeedEnabled ?? false;
            PreassureEnabled = settings?.PreassureEnabled ?? false;


            PropertyChanged += (s,e) => { if (!new string[]{ nameof(WasChangedData), nameof(SaveButtonColorBrush) }.Contains(e.PropertyName)) WasChangedData = true; };
            WasChangedData = false;
        }

        private int equipmentIdentifier;
        public int EquipmentIdentifier { get => equipmentIdentifier; set { ValidateDetectorIdentifier(value); SetProperty(ref equipmentIdentifier, value); OnPropertyChanged(); } }
        ////серийный номер 
        private long equipmentSerialNumber;
        public long EquipmentSerialNumber { get => equipmentSerialNumber; set { SetProperty(ref equipmentSerialNumber, value); } }

        private bool? pressMeteoStationEnabled;

        public int Id { get; set; }

        private List<Tuple<string, string>> accessedPorts = new();
        public List<Tuple<string, string>> AccessedPorts { get => accessedPorts; set => SetProperty(ref accessedPorts, value);}

        public List<Tuple<string, int>> AccessedBoudRates => new List<Tuple<string, int>>()
        {
            Tuple.Create("9600", 9600),
            Tuple.Create("14400", 14400),
            Tuple.Create("19200", 19200),
            Tuple.Create("56000", 56000),
            Tuple.Create("57600", 57600),
            Tuple.Create("115200", 115200),
            Tuple.Create("128000", 128000),
            Tuple.Create("256000", 256000)
        };

        private string portName = SerialPort.GetPortNames().FirstOrDefault() ?? string.Empty;
        public string PortName { get => portName; set => SetProperty(ref portName, value); }

        private int baudRate = 9600;
        public int BaudRate { get => baudRate; set => SetProperty(ref baudRate, value); }

        private bool meteoStationEnabled;
        public bool MeteoStationEnabled 
        { 
            get => meteoStationEnabled; 
            set 
            { 
                SetProperty(ref meteoStationEnabled, value);
                if (!pressMeteoStationEnabled.HasValue)
                {
                    pressMeteoStationEnabled = true;

                    HumidityEnabled = value;
                    TemperatureEnabled = value;
                    WindDirectionEnabled = value;
                    WindSpeedEnabled = value;
                    PreassureEnabled = value;

                    pressMeteoStationEnabled = null;
                }
            } 
        }
        private bool temperatureEnabled;
        public bool TemperatureEnabled 
        { 
            get => temperatureEnabled; 
            set 
            { 
                SetProperty(ref temperatureEnabled, value); SetMeteoStationEnabled(value); 
            } 
        }

        private bool preassureEnabled;
        public bool PreassureEnabled
        {
            get => preassureEnabled;
            set
            {
                SetProperty(ref preassureEnabled, value); SetMeteoStationEnabled(value);
            }
        }

        private bool humidityEnabled;
        public bool HumidityEnabled 
        { 
            get => humidityEnabled; 
            set 
            { 
                SetProperty(ref humidityEnabled, value); SetMeteoStationEnabled(value); 
            } 
        }
        
        private bool windDirectionEnabled;
        public bool WindDirectionEnabled 
        { 
            get => windDirectionEnabled; 
            set 
            { 
                SetProperty(ref windDirectionEnabled, value); SetMeteoStationEnabled(value); 
            } 
        }
        private bool windSpeedEnabled;
        public bool WindSpeedEnabled 
        { 
            get => windSpeedEnabled; 
            set 
            { 
                SetProperty(ref windSpeedEnabled, value); SetMeteoStationEnabled(value); 
            } 
        }

        private Brush saveButtonColorBrush;
        public Brush SaveButtonColorBrush { get => saveButtonColorBrush; set { SetProperty(ref saveButtonColorBrush, value); } }
        private bool wasChangedData;
        public bool WasChangedData
        {
            get => wasChangedData;
            set
            {
                SetProperty(ref wasChangedData, value);
                if (value) SaveButtonColorBrush = new SolidColorBrush(Color.FromArgb(255, 0, 95, 184));
                else SaveButtonColorBrush = new SolidColorBrush(Color.FromArgb(255, 94, 166, 238));
            }
        }

        private void SetMeteoStationEnabled(bool value)
        {
            if(!pressMeteoStationEnabled.HasValue)
                pressMeteoStationEnabled = false;

            if (value)
            {
                MeteoStationEnabled = true;
            }
            else if(!PreassureEnabled && !TemperatureEnabled && !WindDirectionEnabled && !WindSpeedEnabled && !HumidityEnabled)
            {
                MeteoStationEnabled = false;
            }

            if(pressMeteoStationEnabled == false)
                pressMeteoStationEnabled = null;
        }

        public ICommand SaveChangesCommand => new RelayCommand(async () => 
        {
            if (!ValidateModel()) return;

            await _meteoSettingsRepository.AddOrUpdateMeteoSettingsAsync(new MeteoStationSettings
            {
                Id = Id,
                SerialPortName = PortName,
                BaudRate = BaudRate,
                WindSpeedEnabled = WindSpeedEnabled,
                WindDirectionEnabled = WindDirectionEnabled,    
                HumidityEnabled = HumidityEnabled,
                TemperatureEnabled = TemperatureEnabled,
                PreassureEnabled = PreassureEnabled,
                SerialNumber = EquipmentSerialNumber,
                EquipmentIdentifier = EquipmentIdentifier
            });

            if (!_appSettings.IsDevelopment)
            {
                _equipment.Connect(EquipmentType.METEO, true);
            }

            WasChangedData = false;
        });

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
            foreach (var key in _validationErrors.Keys)
            {
                SetErrors(key, new List<string>());
            }
        }

        public bool ValidateDetectorIdentifier(double identifier)
        {
            var errors = new List<string>(1);
            if (identifier < 0 || identifier > 255)
            {
                errors.Add("Ідентифікатор має бути від 0 до 255!");
            }

            SetErrors(nameof(EquipmentIdentifier), errors);

            return !HasErrors;
        }

        public bool ValidateModel()
        {
            return !new bool[]
            {
                ValidateDetectorIdentifier(EquipmentIdentifier)
            }.Any(x => !x);
        }
    }

    public class SerialPortConfigVm : ObservableRecipient
    {
        private List<Tuple<string, string>> accessedPorts = new();
        public List<Tuple<string, string>> AccessedPorts { get => accessedPorts; set => SetProperty(ref accessedPorts, value); }

        public List<Tuple<string, int>> AccessedBoudRates => new List<Tuple<string, int>>()
        {
            Tuple.Create("9600", 9600),
            Tuple.Create("14400", 14400),
            Tuple.Create("19200", 19200),
            Tuple.Create("56000", 56000),
            Tuple.Create("57600", 57600),
            Tuple.Create("115200", 115200),
            Tuple.Create("128000", 128000),
            Tuple.Create("256000", 256000)
        };

        public int Id { get; set; }
        private int baudRate = 9600;
        public int BaudRate { get => baudRate; set => SetProperty(ref baudRate, value); }
        private string portName = SerialPort.GetPortNames().FirstOrDefault() ?? string.Empty;
        public string PortName { get => portName; set => SetProperty(ref portName, value); }
        private int dataBits;
        public int DataBits { get => dataBits; set => SetProperty(ref dataBits, value); }
        private int parity;
        public int Parity { get => parity; set => SetProperty(ref parity, value); }
        private int stopBits;
        public int StopBits { get => stopBits; set => SetProperty(ref stopBits, value); }
    }

    public class SoundAlarmConfigVm : ObservableRecipient
    {
        private int duration;
        public int Duration { get => duration; set => SetProperty(ref duration, value); }

        private int blackoutPeriod;
        public int BlackoutPeriod { get => blackoutPeriod; set => SetProperty(ref blackoutPeriod, value); }

        private int repetitionInterval;
        public int RepetitionInterval { get => repetitionInterval; set => SetProperty(ref repetitionInterval, value); }
    }

    public class MapConfigVm : ObservableRecipient
    {
        public MapConfigVm()
        {
            MapLoadIsActive = mapLoadIsActive;
        }

        private double minLon = 22.10166;
        public double MinLon { get => minLon; set { SetProperty(ref minLon, value); } }
        private double minLat = 44.02738;
        public double MinLat { get => minLat; set { SetProperty(ref minLat, value); } }
        private double maxLon = 40.25842;
        public double MaxLon { get => maxLon; set { SetProperty(ref maxLon, value); } }
        private double maxLat = 52.42326;
        public double MaxLat { get => maxLat; set { SetProperty(ref maxLat, value); } }
        private int minZoomLvl = 0;
        public int MinZoomLvl { get => minZoomLvl; set { SetProperty(ref minZoomLvl, value); } }
        private int maxZoomLvl = 18;
        public int MaxZoomLvl { get => maxZoomLvl; set { SetProperty(ref maxZoomLvl, value); } }
        private bool mapLoadIsActive;
        public bool MapLoadIsActive 
        { 
            get => mapLoadIsActive; 
            private set 
            {
                if (value)
                {
                    BtnContent = "Відмінити завантаження";
                }
                else
                {
                    BtnContent = "Завантажити мапу";
                }

                SetProperty(ref mapLoadIsActive, value); 
            } 
        }
        private string btnContent;
        public string BtnContent { get => btnContent; set => SetProperty(ref btnContent, value); }

        private CancellationTokenSource? cancellationTokenSource;

        public ICommand StartOrCancelLoadMapCommand => new RelayCommand(() => {
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            if(!MapLoadIsActive)
            {
                cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;
                MapLoadIsActive = true;

                Task.Run(() => {
                    try
                    {
                        MapTileCacheService cacheService = App.GetService<MapTileCacheService>();

                        var layer = new TileLayer(KnownTileSources.Create(KnownTileSource.OpenStreetMap, minZoomLevel: MinZoomLvl, maxZoomLevel: MaxZoomLvl),
                             dataFetchStrategy: new DataFetchStrategy());

                        MPoint min = SphericalMercator.FromLonLat(MinLon, MinLat).ToMPoint();
                        MPoint max = SphericalMercator.FromLonLat(MaxLon, MaxLat).ToMPoint();

                        for (int zoomLvl = MinZoomLvl; zoomLvl <= MaxZoomLvl && !token.IsCancellationRequested; zoomLvl++)
                        {
                            var infos = ((HttpTileSource)layer.TileSource).Schema.GetTileInfos(extent: new BruTile.Extent(min.X, min.Y, max.X, max.Y), zoomLvl);

                            foreach (var info in infos)
                            {
                                if (token.IsCancellationRequested) break;
                                cacheService.Add(info.Index, ((HttpTileSource)layer.TileSource).GetTileAsync(info).Result);
                            }
                        }
                    }
                    catch (Exception exception)
                    {

                    }
                    finally
                    {
                        dispatcherQueue.TryEnqueue(() => MapLoadIsActive = false);
                        cancellationTokenSource?.Cancel();
                        cancellationTokenSource?.Dispose();
                        cancellationTokenSource = null;
                    }
                }, token);
            }
            else
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        });
    }
}
