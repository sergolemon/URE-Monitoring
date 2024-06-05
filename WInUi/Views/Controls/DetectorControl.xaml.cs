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
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using URE.ViewModels.Controls;
using URE.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace URE.Views.Controls
{
    public sealed partial class DetectorControl : UserControl
    {
        public DetectorControl()
        {
            this.InitializeComponent();
        }

        public DetectorVm ViewModel => (DetectorVm)DataContext;

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var ts = sender as ToggleSwitch;
            if (ts == null) return;

            if (ts.IsOn)
            {
                toggleSwitchToolTipText.Text = "TooltipTextDetectorGraphOff".GetLocalized();
            }
            else
            {
                toggleSwitchToolTipText.Text = "TooltipTextDetectorGraphOn".GetLocalized();
            }
        }
    }
}
