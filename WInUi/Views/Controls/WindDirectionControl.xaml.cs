using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using URE.ViewModels.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace URE.Views.Controls
{
    public sealed partial class WindDirectionControl : UserControl
    {
        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(nameof(Direction), typeof(double), typeof(WindDirectionControl), new PropertyMetadata(0));
        public double Direction { get => (double)GetValue(DirectionProperty); set => SetValue(DirectionProperty, value); }

        public static readonly DependencyProperty SpeedProperty = DependencyProperty.Register(nameof(Speed), typeof(double), typeof(WindDirectionControl), new PropertyMetadata(0));
        public double Speed { get => (double)GetValue(SpeedProperty); set => SetValue(SpeedProperty, value); }

        public static readonly DependencyProperty DirectionEnabledProperty = DependencyProperty.Register(nameof(DirectionEnabled), typeof(bool), typeof(WindDirectionControl), new PropertyMetadata(true, DirectionEnabledChangedCallback));
        public bool DirectionEnabled { get => (bool)GetValue(DirectionEnabledProperty); 
            set 
            { 
                SetValue(DirectionEnabledProperty, value); 
            } 
        }

        private static void SpeedEnabledChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var obj = o as WindDirectionControl;
            if (obj != null)
            {
                if (!(bool)args.NewValue)
                {
                    obj.unitsTextBlock.Visibility = Visibility.Collapsed;
                    obj.valueTextBlock.Visibility = Visibility.Collapsed;
                }
                else
                {
                    obj.unitsTextBlock.Visibility = Visibility.Visible;
                    obj.valueTextBlock.Visibility = Visibility.Visible;
                }
            }
        }

        private static void DirectionEnabledChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var obj = o as WindDirectionControl;
            if (obj != null)
            {
                if (!(bool)args.NewValue)
                {
                    obj.arrow.Opacity = 0;
                }
                else
                {
                    obj.arrow.Opacity = 1;
                }
            }
        }

        public static readonly DependencyProperty SpeedEnabledProperty = DependencyProperty.Register(nameof(SpeedEnabled), typeof(bool), typeof(WindDirectionControl), new PropertyMetadata(true, SpeedEnabledChangedCallback));
        public bool SpeedEnabled { get => (bool)GetValue(SpeedEnabledProperty); 
            set 
            { 
                SetValue(SpeedEnabledProperty, value); 
            } 
        }

        public WindDirectionControl()
        {
            this.InitializeComponent();
            SizeChanged += (s,e) => {
                arrow.CenterPoint = new System.Numerics.Vector3((float)arrow.ActualWidth / 2, (float)arrow.ActualWidth / 2, 0);    
            };

            Loaded += (s, e) => {
                var transition = new ScalarTransition();
                transition.Duration = TimeSpan.FromSeconds(0.5);
                arrow.RotationTransition = transition;
                arrow.CenterPoint = new System.Numerics.Vector3((float)arrow.ActualWidth / 2, (float)arrow.ActualWidth / 2, 0);
            };

            arrow.Loaded += (s, e) => { 
            
            };
        }
    }
}
