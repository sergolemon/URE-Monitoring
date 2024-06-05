using BruTile;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.ConditionalDraw;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using Mapsui.Rendering;
using Microsoft.UI.Xaml.Shapes;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Models.Equipment;
using URE.Core.Models.Meteo;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.SkiaSharpView.Drawing;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System.Drawing;

namespace URE.ViewModels.Controls
{
    public class HistogramVm : ObservableRecipient
    {
        public double MinXPointLimit { get; }
        public const double MaxAutoXPointLimit = 100;
        public const double MaxManualXPointLimit = 9;
        public const double ZoomStep = 3;
        public int MaxXLimit => Series.Where(x => x.Values != null).Select(x => (x.Values as IEnumerable<(int Id, double? Value)>).Count()).Max();
        public bool IsAutoTrack => !ManualDoseRateValues.Any();
        private readonly GMSettings _gmSettings;
        private int _selectedPointId;

        public IEnumerable<HistogramLineVm> DoseRateLines { get; }
        public HistogramLineVm MoveSpeedLine { get; }
        public ObservableCollection<(int Id, double? Value)> ManualDoseRateValues { get; } = new();
        public ISeries[] Series { get; private set; }
        public IEnumerable<Axis> XAxes { get; }
        public IEnumerable<Axis> YAxes { get; }

        public event Action<int>? PointClicked;

        public int SeletctedPointId {
            get { return _selectedPointId; }
            set
            {
                _selectedPointId = value;
                OnPropertyChanged(nameof(SeletctedPointId));
            }
        }

        public HistogramVm(double maxXPointLimit, HistogramLineVm[] doseRateLines, HistogramLineVm moveSpeedLine, GMSettings gmSettings)
        {
            MinXPointLimit = maxXPointLimit;
            _gmSettings = gmSettings;
            DoseRateLines = doseRateLines;
            MoveSpeedLine = moveSpeedLine;
            InitSeries();
            XAxes = new[]
            {
                new Axis()
                {
                    MinLimit = 0,
                    MaxLimit = maxXPointLimit,
                    Labels = new List<string>(),
                    MinZoomDelta = 2
                }
            };
            YAxes = new[] 
                { 
                    new Axis() 
                    { 
                        Name = "Доза радіації (мкЗв/год)", 
                        NameTextSize = 16, 
                        NamePaint = new SolidColorPaint(SKColors.Black),
                        LabelsPaint = new SolidColorPaint(SKColors.Black),
                        NamePadding = new LiveChartsCore.Drawing.Padding(2, 2),
                        MinLimit = 0
                    }, 
                    new Axis() 
                    { 
                        Name = "Швидкысть руху (км/год)",
                        Position = LiveChartsCore.Measure.AxisPosition.End,
                        NameTextSize = 16,
                        NamePaint = new SolidColorPaint(MoveSpeedLine.Color),
                        LabelsPaint = new SolidColorPaint(MoveSpeedLine.Color),
                        NamePadding = new LiveChartsCore.Drawing.Padding(2, 2)
                    } 
                };
        }

        public void AddDetectorDoseRatePoint(int lineIndex, int id, double? value)
        {
            if (DoseRateLines.Count() - 1 < lineIndex || lineIndex < 0) throw new ArgumentOutOfRangeException();
            ((HistogramLineVm[])DoseRateLines)[lineIndex].Values.Add((id, value));

            var doseRateAxis = ((Axis[])YAxes)[0];
            double max = DoseRateLines.SelectMany(x => x.Values.Select(y => y.Value.HasValue ? y.Value.Value : 0)).Max();
            doseRateAxis.MaxLimit = max + 0.5 + (max/10);
        }

        public void AddManualDoseRatePoint(int id, double? value)
        {
            ManualDoseRateValues.Add((id, value));

            (Series.Last() as ColumnSeries<(int Id, double? Value)>).OnPointMeasured(point =>
            {
                if (point.Visual is null || !point.Model.Value.HasValue) return;

                switch (_gmSettings.GetValueState(point.Model.Value.Value)) 
                {
                    case SettingsValueState.Critical: 
                        {
                            point.Visual.Fill = new SolidColorPaint(SKColors.Red);
                            
                            break; 
                        }
                    case SettingsValueState.High: 
                        {
                            point.Visual.Fill = new SolidColorPaint(SKColors.Yellow);

                            break; 
                        }
                }
            });

            var doseRateAxis = ((Axis[])YAxes)[0];
            double max = ManualDoseRateValues.Select(x => x.Value.HasValue ? x.Value.Value : 0).Max();
            doseRateAxis.MaxLimit = max + 0.5 + (max/10);
        }

        public void AddMoveSpeedPoint(int id, double value)
        {
            MoveSpeedLine.Values.Add((id, value));
        }

        public void AddXLabel(string text)
        {
            var xAxes = ((Axis[])XAxes)[0];
            xAxes.Labels.Add(text);
            if (xAxes.Labels.Count > MinXPointLimit + 1)
            {
                xAxes.MinLimit++;
                xAxes.MaxLimit++;
            }
        }

        public void SetVisibilityOfDoseRateLine(int lineIndex, bool enabled)
        {
            if (DoseRateLines.Count() - 1 < lineIndex || lineIndex < 0) throw new ArgumentOutOfRangeException();

            var series = ((ISeries<(int Id, double? Value)>[])Series)[lineIndex];

            if (enabled)
            {
                var line = ((HistogramLineVm[])DoseRateLines)[lineIndex];

                series.Values = line.Values;
            }
            else
            {
                series.Values = null;   
            }
        }

        public void SetVisibilityOfMoveSpeedLine(bool enabled)
        {
            var series = ((ISeries<(int Id, double? Value)>[])Series)[Series.Count() - 2];

            series.IsVisible = enabled;

            YAxes.Last().IsVisible = enabled;
        }

        public void ClearHistogram()
        {
            foreach (var item in DoseRateLines) 
            {
                item.Values.Clear();
            }

            MoveSpeedLine.Values.Clear();
            ManualDoseRateValues.Clear();

            var xAxes = ((Axis[])XAxes)[0];
            xAxes.Labels.Clear();
            xAxes.MinLimit = 0;
            xAxes.MaxLimit = MinXPointLimit;
        }

        public void GoToStart()
        {
            var xAxes = ((Axis[])XAxes)[0];
            xAxes.MinLimit = 0;
            xAxes.MaxLimit = MinXPointLimit;
        }

        public void ShowPointInfo(int id)
        {
            SeletctedPointId = id;
        }

        private void InitSeries()
        {
            if (DoseRateLines == null) return;

            Series = new ISeries<(int Id, double? Value)>[DoseRateLines.Count() + 2];
            LineSeries<(int Id, double? Value)> lineSeries;

            for (int i = 0; i < DoseRateLines.Count(); i++)
            {
                var line = ((HistogramLineVm[])DoseRateLines)[i];

                lineSeries = new LineSeries<(int Id, double? Value)>
                {
                    Values = line.Values,
                    Stroke = new SolidColorPaint(line.Color, 2),
                    Name = line.Name,
                    LineSmoothness = 0,
                    GeometrySize = 3,
                    GeometryStroke = new SolidColorPaint(line.Color, 2),
                    GeometryFill = new SolidColorPaint(SKColors.White, 3),
                    ScalesYAt = 0,
                    DataLabelsFormatter = (point) => $"{point.Model.Value}",
                    Mapping = (data, index) => data.Value.HasValue ? new(index, data.Value.Value) : Coordinate.Empty
                };

                lineSeries.ChartPointPointerDown += OnLineSeriesPointerDown;
                ((ISeries<(int Id, double? Value)>[])Series)[i] = lineSeries;
            }

            lineSeries = new LineSeries<(int Id, double? Value)>
            {
                Values = MoveSpeedLine.Values,
                Stroke = new SolidColorPaint(MoveSpeedLine.Color, 3),
                Name = MoveSpeedLine.Name,
                LineSmoothness = 0,
                GeometrySize = 5,
                GeometryStroke = new SolidColorPaint(MoveSpeedLine.Color, 3),
                GeometryFill = new SolidColorPaint(SKColors.White, 5),
                ScalesYAt = 1,
                DataLabelsFormatter = (point) => $"{point.Model.Value}",
                Mapping = (data, index) => data.Value.HasValue ? new(index, data.Value.Value) : Coordinate.Empty
            };

            lineSeries.ChartPointPointerDown += OnLineSeriesPointerDown;
            ((ISeries<(int Id, double? Value)>[])Series)[DoseRateLines.Count()] = lineSeries;

            ColumnSeries<(int Id, double? Value)> columnSeries = new ColumnSeries<(int Id, double? Value)>
            {
                Values = ManualDoseRateValues,
                Stroke = new SolidColorPaint(SKColors.Green, 0),
                Fill = new SolidColorPaint(SKColors.Green, 0),
                Name = "Доза радіації",
                ScalesYAt = 0,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsFormatter = (point) => $"{point.Model.Value}",
                Mapping = (data, index) => data.Value.HasValue ? new(index, data.Value.Value) : Coordinate.Empty,
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                IsHoverable = false,
            };

            columnSeries.ChartPointPointerDown += OnColumnSeriesPointerDown;
            ((ISeries<(int Id, double? Value)>[])Series)[DoseRateLines.Count() + 1] = columnSeries;

            ScrollbarSeries = new ISeries[0];

            Thumbs = new[]
            {
            new RectangularSection
            {
                Fill = new SolidColorPaint(SKColors.LightGray)
            }
        };

            InvisibleX = new[] { new Axis { IsVisible = false } };
            InvisibleY = new[] { new Axis { IsVisible = false } };

            // force the left margin to be 100 and the right margin 50 in both charts, this will
            // align the start and end point of the "draw margin",
            // no matter the size of the labels in the Y axis of both chart.
            var auto = LiveChartsCore.Measure.Margin.Auto;
            Margin = new(100, auto, 50, auto);
        }

        private void OnLineSeriesPointerDown(IChartView chart, ChartPoint<(int Id, double? Value), CircleGeometry, LabelGeometry>? point)
        {
            if (point?.Visual is null) return;
            PointClicked?.Invoke(point.Model.Id);
        }

        private void OnColumnSeriesPointerDown(IChartView chart, ChartPoint<(int Id, double? Value), RoundedRectangleGeometry, LabelGeometry>? point)
        {
            if (point?.Visual is null) return;
            PointClicked?.Invoke(point.Model.Id);
        }

        private bool _isDown = false;

        public ISeries[] ScrollbarSeries { get; set; }
        public Axis[] InvisibleX { get; set; }
        public Axis[] InvisibleY { get; set; }
        public LiveChartsCore.Measure.Margin Margin { get; set; }
        public RectangularSection[] Thumbs { get; set; }

        private SolidColorBrush plusBtnBackgroundBrush = new SolidColorBrush(Colors.White);
        public SolidColorBrush PlusBtnBackgroundBrush { get => plusBtnBackgroundBrush; set => SetProperty(ref plusBtnBackgroundBrush, value); }

        private SolidColorBrush minusBtnBackgroundBrush = new SolidColorBrush(Colors.White);
        public SolidColorBrush MinusBtnBackgroundBrush { get => minusBtnBackgroundBrush; set => SetProperty(ref minusBtnBackgroundBrush, value); }

        private SolidColorBrush plusBtnBorderBrush = new SolidColorBrush(Colors.LightGray);
        public SolidColorBrush PlusBtnBorderBrush { get => plusBtnBorderBrush; set => SetProperty(ref plusBtnBorderBrush, value); }

        private SolidColorBrush minusBtnBorderBrush = new SolidColorBrush(Colors.LightGray);
        public SolidColorBrush MinusBtnBorderBrush { get => minusBtnBorderBrush; set => SetProperty(ref minusBtnBorderBrush, value); }

        private SolidColorBrush plusBtnForegroundBrush = new SolidColorBrush(Colors.Black);
        public SolidColorBrush PlusBtnForegroundBrush { get => plusBtnForegroundBrush; set => SetProperty(ref plusBtnForegroundBrush, value); }

        private SolidColorBrush minusBtnForegroundBrush = new SolidColorBrush(Colors.Black);
        public SolidColorBrush MinusBtnForegroundBrush { get => minusBtnForegroundBrush; set => SetProperty(ref minusBtnForegroundBrush, value); }

        public bool MinusBtnEnabled { get; set; } = true;
        public bool PlusBtnEnabled { get; set; } = true;

        public ICommand ChartUpdatedCommand 
        { 
            get => new RelayCommand<ChartCommandArgs>(
                (args) => {
                    var cartesianChart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;

                    var x = cartesianChart.XAxes.First();

                    // update the scroll bar thumb when the chart is updated (zoom/pan)
                    // this will let the user know the current visible range
                    var thumb = Thumbs[0];
                   
                    thumb.Xi = x.MinLimit;
                    thumb.Xj = x.MaxLimit;
                   
                    InvisibleX[0].MinLimit = 0;
                    InvisibleX[0].MaxLimit = MaxXLimit;

                    double maxXPointLimit = IsAutoTrack ? MaxAutoXPointLimit : MaxManualXPointLimit;

                    if (x.MaxLimit - x.MinLimit >= maxXPointLimit || (x.MinLimit <= 0 && x.MaxLimit >= MaxXLimit))
                    {
                        MinusBtnBackgroundBrush = new SolidColorBrush(Colors.WhiteSmoke);
                        MinusBtnForegroundBrush = new SolidColorBrush(Colors.LightGray);
                        MinusBtnEnabled = false;
                    }
                    else if(!MinusBtnEnabled)
                    {
                        MinusBtnForegroundBrush = new SolidColorBrush(Colors.Black);
                        MinusBtnBackgroundBrush = new SolidColorBrush(Colors.White);
                        MinusBtnEnabled = true;
                    }

                    if (x.MaxLimit - x.MinLimit <= MinXPointLimit + 1)
                    {
                        PlusBtnBackgroundBrush = new SolidColorBrush(Colors.WhiteSmoke);
                        PlusBtnForegroundBrush = new SolidColorBrush(Colors.LightGray);
                        PlusBtnEnabled = false;
                    }
                    else if(!PlusBtnEnabled)
                    {
                        PlusBtnForegroundBrush = new SolidColorBrush(Colors.Black);
                        PlusBtnBackgroundBrush = new SolidColorBrush(Colors.White);
                        PlusBtnEnabled = true;
                    }
                }); 
        }

        public ICommand PointerDownCommand
        {
            get => new RelayCommand<PointerCommandArgs>(
                (args) => {
                    _isDown = true;
                });
        }

        public ICommand PointerMoveCommand
        {
            get => new RelayCommand<PointerCommandArgs>(
                (args) => {
                    if (!_isDown) return;

                    var chart = (ICartesianChartView<SkiaSharpDrawingContext>)args.Chart;
                    var positionInData = chart.ScalePixelsToData(args.PointerPosition);

                    var thumb = Thumbs[0];
                    var currentRange = thumb.Xj - thumb.Xi;

                    // update the scroll bar thumb when the user is dragging the chart
                    thumb.Xi = positionInData.X - currentRange / 2;
                    thumb.Xj = positionInData.X + currentRange / 2;

                    // update the chart visible range
                    XAxes.ToArray()[0].MinLimit = thumb.Xi;
                    XAxes.ToArray()[0].MaxLimit = thumb.Xj;
                });
        }

        public ICommand PointerUpCommand
        {
            get => new RelayCommand<PointerCommandArgs>(
                (args) => {
                    _isDown = false;
                });
        }
    }

    public class HistogramLineVm : ObservableRecipient
    {
        public HistogramLineVm(byte rColor, byte gColor, byte bColor, string name)
        {
            Name = name;
            Color = new SKColor(rColor, gColor, bColor);
        }

        public HistogramLineVm(SKColor color, string name)
        {
            Name = name;
            Color = color;
        }

        public ObservableCollection<(int Id, double? Value)> Values { get; set; } = new();
        public SKColor Color { get; set; }
        public string Name { get; set; }
    }
}
