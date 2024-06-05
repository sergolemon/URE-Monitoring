using System.Text;
using System.IO.Ports;
using EasyModbus;
using URE.Contracts.Services;
using URE.Helpers;
using URE.Core.Models.Equipment;
using URE.Core.Contracts.Repositories;
using Microsoft.Extensions.Logging;

namespace URE.Services.Equipment
{
    public class DetectorsService: IEquipmentService
    {
        public event Action? OnConnectedChanged;

        private GMSettings _gmSettings;
        private readonly ModbusClient _modbusClient;

        private readonly IGMSettingsRepository _gmSettingsRepository;
        private readonly IAppNotificationService _appNotificationService;
        private readonly ILogger<DetectorsService> _logger;

        private readonly int _powerRevocationTimeout = 50;
        private readonly int _connectionRevocationTimeout = 30;

        private Dictionary<int, DateTime> _detectorPowerRevocationState = new Dictionary<int, DateTime>();
        private Dictionary<int, DateTime> _detectorConnectionRevocationState = new Dictionary<int, DateTime>();

        private bool _pendingReconnect = false;
        private bool _isConnected = false;

        public bool IsConnected
        {
            get => _isConnected && 
                !_detectorConnectionRevocationState.Any() &&
                !_detectorPowerRevocationState.Any(p => DateTime.Now > p.Value.AddSeconds(_powerRevocationTimeout)); 
            set => _isConnected = value;
        }

        public DetectorsService(IGMSettingsRepository gmSettingsRepository,
                                IAppNotificationService appNotificationService,
                                ILogger<DetectorsService> logger)
        {
            _gmSettingsRepository = gmSettingsRepository;
            _gmSettings = gmSettingsRepository.GetGMSettings();
            _modbusClient = new ModbusClient();

            _appNotificationService = appNotificationService;
            _logger = logger;
        }

        public bool Connect(bool reconnect = false)
        {
            if(reconnect && _isConnected)
            {
                _pendingReconnect = true;
            }
            else
            {
                Connect(reconnect, false);
            }

            return _isConnected;
        }

        public object GetData()
        {
            List<double?> values = new List<double?>();

            CheckConnected();
            if (_isConnected)
            {
                foreach (DetectorSettings item in _gmSettings.DetectorSettings)
                {
                    if (!item.IsEnabled)
                    {
                        values.Add(null);
                        continue;
                    }

                    try
                    {
                        _modbusClient.UnitIdentifier = (byte)item.EquipmentIdentifier;
                        _modbusClient.ReadHoldingRegisters(103, 2);

                        double dose = 0;
                        byte[] data = _modbusClient.receiveData;
                        bool isPowerRevocation = data.ToList().Count(v => v == 0) > 2;

                        if (isPowerRevocation)
                        {
                            CheckPowerRevocation(item);
                        }
                        else
                        {
                            Array.Reverse(data);
                            dose = BitConverter.ToSingle(data);

                            _detectorPowerRevocationState.Remove(item.Id);
                        }
                       
                        values.Add(dose);

                        if (_detectorConnectionRevocationState.ContainsKey(item.Id))
                        {
                            _detectorConnectionRevocationState.Remove(item.Id);
                            OnConnectedChanged?.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        values.Add(null);
                        CheckConnectionRevocation(item);
                        
                        _logger.LogError(ex, $"Conection to equipment {item.Name}({item.Id}) is lost");
                    }
                }
            }

            return values;
        }

        private bool Connect(bool reconnect = false, bool disableNotifications = false)
        {
            if (reconnect)
            {
                _modbusClient.Disconnect();
                _gmSettings = _gmSettingsRepository.GetGMSettings();
                _detectorConnectionRevocationState = new Dictionary<int, DateTime>();
                _detectorPowerRevocationState = new Dictionary<int, DateTime>();
            }

            if (IsCompleteSettings())
            {
                try
                {
                    _modbusClient.SerialPort = _gmSettings.SerialPortName;
                    _modbusClient.Baudrate = _gmSettings.BaudRate;
                    _modbusClient.Parity = Parity.None;
                    _modbusClient.StopBits = StopBits.One;
                    _modbusClient.ConnectionTimeout = 1000;

                    _modbusClient.Connect();
                    _isConnected = true;
                    OnConnectedChanged?.Invoke();
                }
                catch (FileNotFoundException)
                {
                    if (!disableNotifications)
                    {
                        _appNotificationService.Show(String.Format("ReviewConnectSettings".GetLocalized(), "Detector_s".GetLocalized()));
                    }

                    _isConnected = false;
                }
            }
            else
            {
                if (!disableNotifications)
                {
                    _appNotificationService.Show(String.Format("AdjustConnectSettings".GetLocalized(), "Detector_s".GetLocalized()));
                }
            }

            return _isConnected;
        }

        private void CheckConnected()
        {
            if (!_modbusClient.Connected)
            {
                if (_isConnected)
                {
                    _isConnected = false;
                    _appNotificationService.Show(String.Format("EquipmentDisconnected".GetLocalized(), "Detectors".GetLocalized()));
                    OnConnectedChanged?.Invoke();
                }
                else
                {
                    if (Connect(disableNotifications: true))
                    {
                        _appNotificationService.Show(String.Format("EquipmentConnected".GetLocalized(), "Detectors".GetLocalized()));
                    }
                }
            }

            if(_modbusClient.Connected && _pendingReconnect)
            {
                Connect(true, true);
                _pendingReconnect = false;
            }
        }

        private bool IsCompleteSettings()
        {
            return _gmSettings.BaudRate > 0 && !string.IsNullOrEmpty(_gmSettings.SerialPortName) && _gmSettings.DetectorSettings.Any();
        }

        private void CheckPowerRevocation(DetectorSettings detector)
        {
            if (!_detectorPowerRevocationState.ContainsKey(detector.Id))
            {
                _detectorPowerRevocationState[detector.Id] = DateTime.Now;
                _appNotificationService.Show(String.Format("PowerLoss".GetLocalized(), detector.Name));
            }

            if (DateTime.Now > _detectorPowerRevocationState[detector.Id].AddSeconds(_powerRevocationTimeout))
            {
                _detectorPowerRevocationState[detector.Id] = DateTime.Now;

                _appNotificationService.Show(String.Format("NoPower".GetLocalized(), detector.Name));
                OnConnectedChanged?.Invoke();
            }
        }

        private void CheckConnectionRevocation(DetectorSettings detector)
        {
            if (!_detectorConnectionRevocationState.ContainsKey(detector.Id) ||
                DateTime.Now >= _detectorConnectionRevocationState[detector.Id].AddSeconds(_connectionRevocationTimeout))
            {
                _detectorConnectionRevocationState[detector.Id] = DateTime.Now;
                _appNotificationService.Show(String.Format("ItemUnavailable".GetLocalized(), detector.Name));
                OnConnectedChanged?.Invoke();
            }
        }
    }
}
