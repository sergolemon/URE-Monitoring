using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using URE.Contracts.Services;
using URE.Contracts.ViewModels;
using URE.Core.Contracts.Repositories;
using URE.Core.Contracts.Services;
using URE.Core.Models.Equipment;
using URE.Core.Models.Meteo;
using URE.Core.Repositories;
using URE.Core.Services;
using URE.Extensions;
using URE.Services;
using URE.ViewModels.Controls;
using Windows.Storage;

namespace URE.ViewModels
{
    public class HistoryVm : ObservableRecipient, INavigationAware
    {
        private List<MeteoStream> _streamsData;
        private int _selectedHistogram;

        public int SelectedHistogram
        {
            get => _selectedHistogram;
            set
            {
                _selectedHistogram = value;
                OnPropertyChanged(nameof(SelectedHistogram));
            }
        }

        public ShellViewModel ShellViewModel { get; private set; }

        public List<MeteoStream> StreamsData { get => _streamsData; set => SetProperty(ref _streamsData, value); }

        public ObservableCollection<StreamVm> Streams { get; } = new();
        public bool IsImportedShow { get; private set; } = false;
        private readonly IMeteoStreamRepository _meteoStreamRepository;
        private readonly IGMSettingsRepository _gmSettingsRepository;
        private readonly IGPSSettingsRepository _gpsSettingsRepository;
        private readonly IMeteoDataService _meteoDataService;

        public HistoryVm(IMeteoStreamRepository meteoStreamRepository, 
                         IGMSettingsRepository gmSettingsRepository,
                         IGPSSettingsRepository gpsSettingsRepository,
                         IMeteoDataService meteoDataService)
        {            
            _meteoStreamRepository = meteoStreamRepository;
            _gmSettingsRepository = gmSettingsRepository;
            _gpsSettingsRepository = gpsSettingsRepository;
            _meteoDataService = meteoDataService;
        }

        private int totalPagesCount;
        public int TotalPagesCount { get => totalPagesCount; set { SetProperty(ref totalPagesCount, value); } }

        public void OnNavigated(ShellViewModel shellViewModel)
        {
            ShellViewModel = shellViewModel;
            ShellViewModel.PointClicked += OnMapPointClicked;

            TotalPagesCount = shellViewModel.TotalPagesCount;
        }

        public void OnNavigatedTo(object parameter){}

        public void OnNavigatedFrom()
        {
            ShellViewModel.RenderHistoryClicked = false;
        }

        public async Task InitialAsync()
        {
            var gmSettings = _gmSettingsRepository.GetGMSettings();
            var gpsSettings = _gpsSettingsRepository.GetGPSSettingsOrNull();

            var detector1Settings = gmSettings.DetectorSettings.ElementAtOrDefault(0);
            var detector2Settings = gmSettings.DetectorSettings.ElementAtOrDefault(1);
            var detector3Settings = gmSettings.DetectorSettings.ElementAtOrDefault(2);
            var detector4Settings = gmSettings.DetectorSettings.ElementAtOrDefault(3);
            var detector5Settings = gmSettings.DetectorSettings.ElementAtOrDefault(4);
            var detector6Settings = gmSettings.DetectorSettings.ElementAtOrDefault(5);

            var moveSpeedColor = ColorExtensions.FromInt(gpsSettings?.MoveSpeedColor ?? 0);

            var detector1LineColor = ColorExtensions.FromInt(detector1Settings?.Color ?? 0);
            var detector2LineColor = ColorExtensions.FromInt(detector2Settings?.Color ?? 0);
            var detector3LineColor = ColorExtensions.FromInt(detector3Settings?.Color ?? 0);
            var detector4LineColor = ColorExtensions.FromInt(detector4Settings?.Color ?? 0);
            var detector5LineColor = ColorExtensions.FromInt(detector5Settings?.Color ?? 0);
            var detector6LineColor = ColorExtensions.FromInt(detector6Settings?.Color ?? 0);

            //var streams = await _meteoStreamRepository.GetByDates(SelectedDates.Select(x => x.Date).OrderByDescending(x => x).ToList());

            Dictionary<int, (bool Auto, StreamVm StreamVm)> renderStreams = new Dictionary<int, (bool Auto, StreamVm streamVm)>();
            List<MeteoData> meteoData = new List<MeteoData>();

            foreach(MeteoStream stream in StreamsData)
            {
                var linesArr = new HistogramLineVm[]
               {
                    new HistogramLineVm(new SKColor(detector1LineColor.R, detector1LineColor.G, detector1LineColor.B), detector1Settings.Name),
                    new HistogramLineVm(new SKColor(detector2LineColor.R, detector2LineColor.G, detector2LineColor.B), detector2Settings.Name),
                    new HistogramLineVm(new SKColor(detector3LineColor.R, detector3LineColor.G, detector3LineColor.B), detector3Settings.Name),
                    new HistogramLineVm(new SKColor(detector4LineColor.R, detector4LineColor.G, detector4LineColor.B), detector4Settings.Name),
                    new HistogramLineVm(new SKColor(detector5LineColor.R, detector5LineColor.G, detector5LineColor.B), detector5Settings.Name),
                    new HistogramLineVm(new SKColor(detector6LineColor.R, detector6LineColor.G, detector6LineColor.B), detector6Settings.Name)
               };

                HistogramVm histogram = new(5, linesArr, new HistogramLineVm(moveSpeedColor.R, moveSpeedColor.G, moveSpeedColor.B, "Швидкість руху"), gmSettings);
                histogram.PointClicked += OnHistogramPointClicked;

                var renderStream = new StreamVm
                {
                    StreamId = stream.Id,
                    StreamPeriodStr = $"{stream.DateStart} - {stream.DateEnd}",
                    Histogram = histogram,
                    PersonInfo = stream.User?.PersonInfo!
                };

                renderStreams.Add(stream.Id, (stream.Auto, renderStream));
                meteoData.AddRange(stream.Data);
            }

            foreach(MeteoData data in meteoData)
            {
                (bool Auto, StreamVm StreamVm) stream = renderStreams[data.MeteoStreamId];
                if (stream.Auto)
                {
                    stream.StreamVm.Histogram.AddMoveSpeedPoint(data.Id, data.Speed);
                    stream.StreamVm.Histogram.AddDetectorDoseRatePoint(0, data.Id, data.D1Radiation.HasValue ? Math.Round(data.D1Radiation.Value, 3) : null);
                    stream.StreamVm.Histogram.AddDetectorDoseRatePoint(1, data.Id, data.D2Radiation.HasValue ? Math.Round(data.D2Radiation.Value, 3) : null);
                    stream.StreamVm.Histogram.AddDetectorDoseRatePoint(2, data.Id, data.D3Radiation.HasValue ? Math.Round(data.D3Radiation.Value, 3) : null);
                    stream.StreamVm.Histogram.AddDetectorDoseRatePoint(3, data.Id, data.D4Radiation.HasValue ? Math.Round(data.D4Radiation.Value, 3) : null);
                    stream.StreamVm.Histogram.AddDetectorDoseRatePoint(4, data.Id, data.D5Radiation.HasValue ? Math.Round(data.D5Radiation.Value, 3) : null);
                    stream.StreamVm.Histogram.AddDetectorDoseRatePoint(5, data.Id, data.D6Radiation.HasValue ? Math.Round(data.D6Radiation.Value, 3) : null);
                }
                else
                {
                    stream.StreamVm.Histogram.SetVisibilityOfMoveSpeedLine(false);
                    stream.StreamVm.Histogram.AddManualDoseRatePoint(data.Id, Math.Round(data.ManualInputRadiation, 3));
                }

                stream.StreamVm.Histogram.AddXLabel((data.Date + data.Time).ToLongTimeString());
                stream.StreamVm.Histogram.GoToStart();
            }

            foreach(var renderStream in renderStreams)
            {
                Streams.Add(renderStream.Value.StreamVm);
            }
        }

        public void Export(string path)
        {
            _meteoDataService.Export(path, StreamsData);
        }

        public async Task<string> Import(IReadOnlyList<StorageFile> files)
        {
            var result = await _meteoDataService.Import(files, ShellViewModel.ImportedStreams);
            TotalPagesCount = (int)Math.Ceiling(ShellViewModel.ImportedStreams.Count / (decimal)ShellViewModel.StreamsCountOnPage);
            IsImportedShow = true;
            return result;
        }

        private void OnHistogramPointClicked(int id)
        {
            ShellViewModel.PointToScale = id;
        }

        private void OnMapPointClicked(MeteoData data)
        {
            StreamVm streamVm = Streams.Where(s => s.StreamId == data.MeteoStreamId).FirstOrDefault();
            if(streamVm != null)
            {
                SelectedHistogram = Streams.IndexOf(streamVm);
                streamVm.Histogram.ShowPointInfo(data.Id);
            }
        }
    }

    public class StreamVm
    {
        public int StreamId { get; set; }
        public string StreamPeriodStr { get; set;}
        public string PersonInfo { get; set; }
        public HistogramVm Histogram { get; set;}
    }
}
