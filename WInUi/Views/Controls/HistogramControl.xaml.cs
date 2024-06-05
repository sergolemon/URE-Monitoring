using Microsoft.UI;
using LiveChartsCore.Kernel;
using LiveChartsCore.Drawing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using URE.ViewModels.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using System.ComponentModel;
using URE.ViewModels.Controls;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace URE.Views.Controls
{
    public sealed partial class HistogramControl : UserControl
    {
        HistogramVm ViewModel => (HistogramVm)DataContext;

        public HistogramControl()
        {
            this.InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs e)
        {
            if (e.NewValue != null)
                ViewModel.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SeletctedPointId):
                    ShowPoint();
                    break;
                default:
                    break;

            }
        }

        private void ShowPoint()
        { 
            if (Chart.Series.Any())
            {
                List<ChartPoint> points = new List<ChartPoint>();

                foreach (var series in Chart.Series)
                {
                    IList<(int, double?)> values = series.Values as IList<(int, double?)>;
                    if (values != null && values.Any())
                    {
                        var pointValue = values.FirstOrDefault(v => v.Item1 == ViewModel.SeletctedPointId);
                        int pointIndex = values.IndexOf(pointValue);

                        ChartPoint chartPoint = series.Fetch(Chart.CoreChart).FirstOrDefault(p => p.Index == pointIndex);

                        if (chartPoint != null)
                        {
                            MoveToPoint(pointIndex);
                            ScalePoint(chartPoint, 2.5);

                            points.Add(chartPoint);
                        }
                    }
                }

                Chart.CoreChart.Update();

                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        foreach (var point in points)
                        {
                            ScalePoint(point, 1);
                        }

                        Chart.CoreChart.Update();
                    });
                });
            }
        }

        private void ScalePoint(ChartPoint point, double scale)
        {
            var series = point.Context.Series;
            if (series as LineSeries<(int, double?)> != null)
            {
                var linePointVisual = point.Context.Visual as CircleGeometry;
                if (linePointVisual != null)
                {
                    linePointVisual.ScaleTransform = new LvcPoint(scale, scale);
                }
            }

            if (series as ColumnSeries<(int, double?)> != null)
            {
                var columnPointVisual = point.Context.Visual as RoundedRectangleGeometry;
                if (columnPointVisual != null)
                {
                    columnPointVisual.ScaleTransform = new LvcPoint(scale, scale);
                }
            }
        }

        private void MoveToPoint(int index)
        {
            while (index >= Chart.XAxes.First().MaxLimit)
            {
                Chart.XAxes.First().MinLimit++;
                Chart.XAxes.First().MaxLimit++;
            }

            while (index <= Chart.XAxes.First().MinLimit)
            {
                Chart.XAxes.First().MinLimit--;
                Chart.XAxes.First().MaxLimit--;
            }
        }

        private void PlusZoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is null) return;

            var model = (HistogramVm)DataContext;
            if (!model.PlusBtnEnabled) return;
            var xAxes = model.XAxes.First();

            if (model.PlusBtnEnabled) xAxes.MinLimit += model.IsAutoTrack ? HistogramVm.ZoomStep : 1;
            if (model.PlusBtnEnabled) xAxes.MaxLimit -= model.IsAutoTrack ? HistogramVm.ZoomStep : 1;

            //var currBorder = model.PlusBtnBorderBrush;
            //var currBackground = model.PlusBtnBackgroundBrush;
            //var currForeground = model.PlusBtnForegroundBrush;

            //model.PlusBtnForegroundBrush = new SolidColorBrush(Colors.White);
            //model.PlusBtnBackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 0, 95, 184));
            //model.PlusBtnBorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 95, 184));

            //Task.Delay(100).ContinueWith((t) => {
            //    DispatcherQueue.TryEnqueue(() => { 
            //    model.PlusBtnForegroundBrush = currForeground;
            //    model.PlusBtnBackgroundBrush = currBackground;
            //    model.PlusBtnBorderBrush = currBorder;
            //    });
            //});
        }

        private void MinusZoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is null) return;

            var model = (HistogramVm)DataContext;
            if (!model.MinusBtnEnabled) return;

            var xAxes = model.XAxes.First();

            if (model.MinusBtnEnabled) xAxes.MinLimit -= model.IsAutoTrack ? HistogramVm.ZoomStep : 1;
            if (model.MinusBtnEnabled) xAxes.MaxLimit += model.IsAutoTrack ? HistogramVm.ZoomStep : 1;

            //var currBorder = model.MinusBtnBorderBrush;
            //var currBackground = model.MinusBtnBackgroundBrush;
            //var currForeground = model.MinusBtnForegroundBrush;

            //model.MinusBtnForegroundBrush = new SolidColorBrush(Colors.White);
            //model.MinusBtnBackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 0, 95, 184));
            //model.MinusBtnBorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 95, 184));

            //Task.Delay(100).ContinueWith((t) => {
            //    DispatcherQueue.TryEnqueue(() => {
            //        model.MinusBtnForegroundBrush = currForeground;
            //        model.MinusBtnBackgroundBrush = currBackground;
            //        model.MinusBtnBorderBrush = currBorder;
            //    });
            //});
        }

        private void MinusButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //var model = (HistogramVm)DataContext;
            //if (!model.MinusBtnEnabled) return;
            //model.MinusBtnForegroundBrush = new SolidColorBrush(Color.FromArgb(255, 0, 95, 184));
            //model.MinusBtnBackgroundBrush = new SolidColorBrush(Colors.WhiteSmoke);
        }

        private void MinusButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //var model = (HistogramVm)DataContext;
            //if (!model.MinusBtnEnabled) return;
            //model.MinusBtnForegroundBrush = new SolidColorBrush(Colors.Black);
            //model.MinusBtnBackgroundBrush = new SolidColorBrush(Colors.White);
        }

        private void PlusButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //var model = (HistogramVm)DataContext;
            //if (!model.PlusBtnEnabled) return;
            //model.PlusBtnForegroundBrush = new SolidColorBrush(Color.FromArgb(255, 0, 95, 184));
            //model.PlusBtnBackgroundBrush = new SolidColorBrush(Colors.WhiteSmoke);
        }

        private void PlusButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //var model = (HistogramVm)DataContext;
            //if (!model.PlusBtnEnabled) return;
            //model.PlusBtnForegroundBrush = new SolidColorBrush(Colors.Black);
            //model.PlusBtnBackgroundBrush = new SolidColorBrush(Colors.White);
        }
    }
}
