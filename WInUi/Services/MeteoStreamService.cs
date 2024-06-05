using Microsoft.UI.Dispatching;
using URE.Contracts.Services;
using URE.Core.Models.Meteo;
using URE.Core.Contracts.Services;
using URE.Core.Contracts.Repositories;

namespace URE.Services
{
    public class MeteoStreamService: IMeteoStreamService
    {
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly IMeteoDataService _meteoDataService;
        private readonly IMeteoDataRepository _meteoDataRepository;
        private readonly IMeteoStreamRepository _meteoStreamRepository;
        private readonly ISignInManager _signInManager;

        private bool _isListeningData;
        private bool _isStreamingData;
        private bool _isInputStreamingData;
        private int _currentStreamId;

        public event Action<MeteoData>? OnDataChanged;
        public event Action<MeteoData>? OnInputDataChanged;

        public event Action<bool>? OnDataStreamingStarted;
        public event Action? OnDataStreamingCompleted;

        private CancellationTokenSource? _cancellationTokenSource;

        public MeteoStreamService(IMeteoDataService meteoDataService,
                                  IMeteoDataRepository meteoDataRepository,
                                  IMeteoStreamRepository meteoStreamRepository,
                                  ISignInManager signInManager)
        {
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _meteoDataService = meteoDataService;
            _meteoDataRepository = meteoDataRepository;
            _meteoStreamRepository = meteoStreamRepository;
            _signInManager = signInManager;
        }

        public void StartListening()
        {
            if (_isListeningData)
                return;

            _isListeningData = true;
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    MeteoData data = _meteoDataService.GetMeteoRow();

                    if (_isStreamingData)
                    {
                        data.MeteoStreamId = _currentStreamId;
                        await _meteoDataRepository.AddMeteoDataAsync(data);
                    }

                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        OnDataChanged?.Invoke(data);
                    });

                    await Task.Delay(1000);
                }
            }, _cancellationTokenSource.Token);
        }

        public void StopListening()
        {
            _cancellationTokenSource?.Cancel();
            _isListeningData = false;
        }

        public async Task StartStreamingAsync(bool manual = false)
        {
            if (!manual)
            {
                _currentStreamId = await _meteoStreamRepository.AddMeteoStreamAsync(
                    new MeteoStream
                    {
                        DateStart = DateTime.Now,
                        UserId = _signInManager.Identity.UserId,
                        Auto = true
                    });

                _isStreamingData = true;
            }
            else
            {
                _isInputStreamingData = true;
            }
           
            OnDataStreamingStarted?.Invoke(!manual);
        }

        public async Task StopStreamingAsync(bool manual = false)
        {
            if (manual && _currentStreamId != 0)
            {
                MeteoData lastEntry = await _meteoDataRepository.GetLastByStreamId(_currentStreamId);
                await _meteoStreamRepository.CloseStream(_currentStreamId, lastEntry.Date + lastEntry.Time);

                _isInputStreamingData = false;

            }
            else
            {
                await _meteoStreamRepository.CloseStream(_currentStreamId, DateTime.Now);
                _isStreamingData = false;
            }

            _currentStreamId = 0;
            OnDataStreamingCompleted?.Invoke();
        }

        public async Task PushData(MeteoData data)
        {
            if (_currentStreamId == 0)
            {
                _currentStreamId = await _meteoStreamRepository.AddMeteoStreamAsync(
                   new MeteoStream
                   {
                       DateStart = data.Date + data.Time,
                       Auto = false,
                       UserId = _signInManager.Identity.UserId
                   });
            }

            if (_isInputStreamingData)
            {
                data.MeteoStreamId = _currentStreamId;
                await _meteoDataRepository.AddMeteoDataAsync(data);
            }

            _dispatcherQueue.TryEnqueue(() =>
            {
                OnInputDataChanged?.Invoke(data);
            });
        }

    }
}
