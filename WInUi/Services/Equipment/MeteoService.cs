using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime;
using System.Text;
using Microsoft.Extensions.Logging;
using EasyModbus;
using URE.Contracts.Services;
using URE.Core.Contracts.Repositories;
using URE.Core.Models.Equipment;
using URE.Helpers;

namespace URE.Services.Equipment
{
    public class MeteoService : IEquipmentService
    {
        public event Action? OnConnectedChanged;

        private MeteoStationSettings _meteoSettings;
        private readonly ModbusClient _modbusClient;

        private readonly IMeteoSettingsRepository _meteoSettingsRepository;
        private readonly IAppNotificationService _appNotificationService;
        private readonly ILogger<MeteoService> _logger;

        private bool _isConnected = false;

        public bool IsConnected
        {
            get => _isConnected;
            set => _isConnected = value;
        }

        public MeteoService(IMeteoSettingsRepository meteoSettingsRepository,
                            IAppNotificationService appNotificationService,
                            ILogger<MeteoService> logger)
        {
            _meteoSettingsRepository = meteoSettingsRepository;
            _meteoSettings = _meteoSettingsRepository.GetMeteoSettingsOrNull();
            _modbusClient = new ModbusClient();

            _appNotificationService = appNotificationService;
            _logger = logger;
        }

        public bool Connect(bool reconnect = false)
        {
            return Connect(reconnect, false);
        }

        public object GetData()
        {
            (double Direction, 
             double WindSpeed, 
             double CorrectedDirection, 
             double Pressure, 
             double RelativeHumidity, 
             double Temperature, 
             double DewPoint, 
             double SupplyVoltage) data = (0, 0, 0, 0, 0, 0, 0, 0);

            CheckConnected();

            if (_isConnected)
            {
                try
                {
                    _modbusClient.UnitIdentifier = 5;

                    data.Direction = GetRegisterValue(2, false);
                    data.WindSpeed = GetRegisterValue(4, true);
                    data.CorrectedDirection = GetRegisterValue(6, false);
                    data.Pressure = GetRegisterValue(10, true);
                    data.RelativeHumidity = GetRegisterValue(12, false);
                    data.Temperature = GetRegisterValue(14, true);
                    data.DewPoint = GetRegisterValue(16, true);
                    data.SupplyVoltage = GetRegisterValue(40, true);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error during getting values from meteo.");
                }
            }
            
            return data;
        }

        public bool Connect(bool reconnect = false, bool disableNotifications = false)
        {
            if (reconnect)
            {
                _modbusClient.Disconnect();
                _meteoSettings = _meteoSettingsRepository.GetMeteoSettingsOrNull();
            }

            if (IsCompleteSettings())
            {
                try
                {
                    _modbusClient.SerialPort = _meteoSettings.SerialPortName;
                    _modbusClient.Baudrate = _meteoSettings.BaudRate;
                    _modbusClient.Parity = Parity.None;
                    _modbusClient.StopBits = StopBits.Two;
                    _modbusClient.ConnectionTimeout = 1000;

                    _modbusClient.Connect();
                    _isConnected = true;
                    OnConnectedChanged?.Invoke();
                }
                catch (Exception)
                {
                    if (!disableNotifications)
                    {
                        _appNotificationService.Show(String.Format("ReviewConnectSettings".GetLocalized(), "Meteo".GetLocalized()));
                    }

                    _isConnected = false;
                }
            }
            else
            {
                if (!disableNotifications)
                {
                    _appNotificationService.Show(String.Format("AdjustConnectSettings".GetLocalized(), "Meteo".GetLocalized()));
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
                    _appNotificationService.Show(String.Format("EquipmentDisconnected".GetLocalized(), "Meteo-own".GetLocalized()));
                    OnConnectedChanged?.Invoke();
                }
                else
                {
                    if (Connect(disableNotifications: true))
                    {
                        _appNotificationService.Show(String.Format("EquipmentConnected".GetLocalized(), "Meteo-own".GetLocalized()));
                    }
                }
            }
        }

        private bool IsCompleteSettings()
        {
            return _meteoSettings.BaudRate > 0 && !string.IsNullOrEmpty(_meteoSettings.SerialPortName);
        }

        private double GetRegisterValue(int address, bool floating)
        {
            int[] registers = _modbusClient.ReadHoldingRegisters(address, 2);

            uint high = (uint)registers[0] << 16;
            uint low = floating ? (uint)(ushort)registers[1] : (uint)registers[1];

            return BitConverter.ToSingle(BitConverter.GetBytes(high | low), 0);
        }
    }
}
