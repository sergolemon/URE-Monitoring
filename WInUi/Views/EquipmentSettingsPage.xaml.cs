using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using URE.ViewModels;
using URE.Core.Models;
using Microsoft.Extensions.Options;
using System.IO.Ports;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace URE.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EquipmentSettingsPage : Page
    {
        public EquipmentSettingsPage()
        {
            DataContext = App.GetService<EquipmentSettingsVm>();

            this.InitializeComponent();
        }

        private void OnLoaded(object? sender, RoutedEventArgs args)
        {
            ViewModel.ComPortConfig.AccessedPorts = GetAccessedComPorts(ViewModel.ComPortConfig.PortName, ViewModel.GpsSettings.PortName, ViewModel.MeteoStationSettings.PortName);
            ViewModel.MeteoStationSettings.AccessedPorts = GetAccessedComPorts(ViewModel.MeteoStationSettings.PortName, ViewModel.GpsSettings.PortName, ViewModel.ComPortConfig.PortName);
            ViewModel.GpsSettings.AccessedPorts = GetAccessedComPorts(ViewModel.GpsSettings.PortName, ViewModel.MeteoStationSettings.PortName, ViewModel.ComPortConfig.PortName);
        }

        private void ComboBox_SerialPort_OnDropDownOpened(object? sender, object args)
        {
            ViewModel.ComPortConfig.AccessedPorts = GetAccessedComPorts(ViewModel.ComPortConfig.PortName, ViewModel.GpsSettings.PortName, ViewModel.MeteoStationSettings.PortName);
        }

        private void ComboBox_Meteostation_OnDropDownOpened(object? sender, object args)
        {
            ViewModel.MeteoStationSettings.AccessedPorts = GetAccessedComPorts(ViewModel.MeteoStationSettings.PortName, ViewModel.GpsSettings.PortName, ViewModel.ComPortConfig.PortName);
        }

        private void ComboBox_Gps_OnDropDownOpened(object? sender, object args)
        {
            ViewModel.GpsSettings.AccessedPorts = GetAccessedComPorts(ViewModel.GpsSettings.PortName, ViewModel.MeteoStationSettings.PortName, ViewModel.ComPortConfig.PortName);
        }

        private List<Tuple<string, string>> GetAccessedComPorts(string currentPort, params string[] ignorablePorts)
        {
            var allPorts = SerialPort.GetPortNames();
            var accessedPorts = allPorts
            .Where(x => {
                    using var sp = new SerialPort(x);

                    try
                    {
                        sp.Open();
                    }
                    catch
                    {
                        return false;
                    }
                    finally { sp.Close(); }

                    return true;
            }).ToList();

            if (allPorts.Contains(currentPort) && !accessedPorts.Any(x => x == currentPort)) accessedPorts.Add(currentPort);

            accessedPorts.ForEach(x => {
                if (ignorablePorts.Contains(x)) accessedPorts.Remove(x);    
            });

            return accessedPorts.Select(x => Tuple.Create(x, x)).ToList();
        }

        public EquipmentSettingsVm ViewModel => (EquipmentSettingsVm)DataContext;
    }
}
