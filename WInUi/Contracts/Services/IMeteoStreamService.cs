using URE.Core.Models.Meteo;

namespace URE.Contracts.Services
{
    public interface IMeteoStreamService
    {
        event Action<MeteoData>? OnDataChanged;
        event Action<MeteoData>? OnInputDataChanged;

        event Action<bool>? OnDataStreamingStarted;
        event Action? OnDataStreamingCompleted;

        void StartListening();
        Task StartStreamingAsync(bool manual = false);
        Task StopStreamingAsync(bool manual = false);
        Task PushData(MeteoData data);

    }
}
