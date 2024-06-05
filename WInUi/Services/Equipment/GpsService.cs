using System.IO.Ports;
using System.Globalization;
using Microsoft.Extensions.Logging;
using URE.Helpers;
using URE.Contracts.Services;
using URE.Core.Contracts.Repositories;
using URE.Core.Models.Equipment;

namespace URE.Services.Equipment
{
    public class GpsService : IEquipmentService
    {
        public event Action? OnConnectedChanged;

        private GPSSettings _gpsSettings;
        private readonly SerialPort _serialPort;

        private readonly IGPSSettingsRepository _gpsSettingsRepository;
        private readonly IAppNotificationService _appNotificationService;
        private readonly ILogger<GpsService> _logger;

        private bool _isConnected = false;

        private (double Lat, double Lon, double Height, double Speed) _gpsData = (0, 0, 0, 0);

        public bool IsConnected
        {
            get => _isConnected;
            set => _isConnected = value;
        }

        public GpsService(IGPSSettingsRepository gpsSettingsRepository,
                          IAppNotificationService appNotificationService,
                          ILogger<GpsService> logger)
        {
            _gpsSettingsRepository = gpsSettingsRepository;
            _gpsSettings = _gpsSettingsRepository.GetGPSSettingsOrNull();
            _serialPort = new SerialPort();

            _appNotificationService = appNotificationService;
            _logger = logger;
        }

        public bool Connect(bool reconnect = false)
        {
            return Connect(reconnect, false);
        }

        public object GetData()
        {
            CheckConnected();
            return _gpsData;
        }

        public bool Connect(bool reconnect = false, bool disableNotifications = false)
        {
            if (reconnect)
            {
                _serialPort.Close();
                _gpsSettings = _gpsSettingsRepository.GetGPSSettingsOrNull();
            }

            if (!string.IsNullOrEmpty(_gpsSettings.SerialPortName))
            {
                try
                {
                    _serialPort.PortName = _gpsSettings.SerialPortName;
                    _serialPort.Open();
                    _serialPort.DataReceived += DataReceivedHandler;
                    _serialPort.ReadTimeout = 1000;
                    _isConnected = true;

                    OnConnectedChanged?.Invoke();
                }
                catch (Exception)
                {
                    if (!disableNotifications)
                    {
                        _appNotificationService.Show(String.Format("ReviewConnectSettings".GetLocalized(), "GPS".GetLocalized()));
                    }

                    _isConnected = false;
                }
            }
            else
            {
                if (!disableNotifications)
                {
                    _appNotificationService.Show(String.Format("AdjustConnectSettings".GetLocalized(), "GPS".GetLocalized()));
                }
            }

            return _isConnected;
        }

        private void CheckConnected()
        {
            if (!_serialPort.IsOpen)
            {
                if (_isConnected)
                {
                    _isConnected = false;
                    _appNotificationService.Show(String.Format("EquipmentDisconnected".GetLocalized(), "GPS-own".GetLocalized()));
                    OnConnectedChanged?.Invoke();
                }
                else
                {
                    if (Connect(disableNotifications: true))
                    {
                        _appNotificationService.Show(String.Format("EquipmentConnected".GetLocalized(), "GPS-own".GetLocalized()));
                    }
                }
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string gpsString = _serialPort.ReadLine();
                string[] data = gpsString.Split(',');

                if (data[0] == "$GNGGA")
                {
                    _gpsData.Lat = FormatCoordinate(double.TryParse(data[2], CultureInfo.InvariantCulture, out double res) ? res : 0);
                    _gpsData.Lon = FormatCoordinate(double.TryParse(data[4], CultureInfo.InvariantCulture, out res) ? res : 0);
                    _gpsData.Height = double.TryParse(data[9], CultureInfo.InvariantCulture, out res) ? res : 0;

                }


                if (data[0] == "$GNRMC")
                {
                    _gpsData.Speed = double.TryParse(data[7], CultureInfo.InvariantCulture, out var res) ? res : 0;
                    _gpsData.Speed = _gpsData.Speed * 1.852;

                }
            }
            catch (Exception)
            {

            }

        }

        private double FormatCoordinate(double coordinate)
        {
            int degrees = (int)(coordinate / 100);
            double minutes = coordinate % 100; 

            double decimalMinutes = minutes / 60;

            double decimalDegrees = degrees + decimalMinutes;

            return decimalDegrees;
        }
    }
}
