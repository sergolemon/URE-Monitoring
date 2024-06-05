using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Contracts.Repositories;
using URE.Core.Models.Equipment;
using URE.Core.Repositories;
using Windows.Media.Core;
using Windows.Media.Playback;
using URE.Contracts.Services;

namespace URE.Services
{
    internal class AlarmService : IAlarmService
    {
        private readonly IGMSettingsRepository _gmSettingsRepository;

        public AlarmService(IGMSettingsRepository gmSettingsRepository)
        {
            _gmSettingsRepository = gmSettingsRepository;
        }

        public AlarmType? PlayingAlarmType { get; private set; }

        public bool IsDoseRateAlarmMute { get; private set; }

        public SettingsValueState? DoseRateAlarmLvl { get; private set; }

        public bool IsGm0AlarmMute { get; private set; }
        public bool IsMeteoAlarmMute { get; private set; }
        public bool IsGpsAlarmMute { get; private set; }

        public EquipmentType? EquipmentAlarmType { get; private set; }

        public bool IsAlarmMute => IsGm0AlarmMute && IsGpsAlarmMute && IsMeteoAlarmMute && IsDoseRateAlarmMute;

        public event Action MuteAlarmStateChanged;
        public event Action PlayingAlarmUpdated;

        private CancellationTokenSource? playingAlarmCancellationTokenSource;
        private CancellationTokenSource? doseRateAlarmMuteCancellationTokenSource;
        private CancellationTokenSource? gm0AlarmMuteCancellationTokenSource;
        private CancellationTokenSource? meteoAlarmMuteCancellationTokenSource;
        private CancellationTokenSource? gpsAlarmMuteCancellationTokenSource;
        private CancellationTokenSource? onMomentSoundCancellationTokenSource;

        public void UpdatePlayingDoseRateAlarm(SettingsValueState alarmLvl)
        {
            if (!(DoseRateAlarmLvl == alarmLvl || (alarmLvl == SettingsValueState.Normal && PlayingAlarmType != AlarmType.DoseRate)))
            {
                if (alarmLvl == SettingsValueState.Normal && PlayingAlarmType == AlarmType.DoseRate)
                {
                    StopAlarm();
                }
                else
                {
                    StopAlarm();

                    var gmSettings = _gmSettingsRepository.GetGMSettings();
                    PlayingAlarmType = AlarmType.DoseRate;
                    DoseRateAlarmLvl = alarmLvl;
                    playingAlarmCancellationTokenSource = new CancellationTokenSource();
                    var token = playingAlarmCancellationTokenSource.Token;
                    string soundPath = string.Empty;

                    switch (alarmLvl)
                    {
                        case SettingsValueState.High:
                            {
                                soundPath = "ms-appx:///Sound/medium.wav";

                                break;
                            }
                        case SettingsValueState.Critical:
                            {
                                soundPath = "ms-appx:///Sound/high.wav";

                                break;
                            }
                    }
     
                    Task.Run(async () =>
                    {
                        while (!token.IsCancellationRequested)
                        {
                            if (IsDoseRateAlarmMute) { await Task.Delay(500, token).ContinueWith(task => { }); continue; }
                            var onMomentSoundCancellationTokenSource = new CancellationTokenSource();
                            this.onMomentSoundCancellationTokenSource = onMomentSoundCancellationTokenSource;
                            using (var player = new MediaPlayer())
                            {
                                player.Source = MediaSource.CreateFromUri(new Uri(soundPath));
                                player.Play();
                                await Task.Delay(TimeSpan.FromSeconds(2.8), onMomentSoundCancellationTokenSource.Token).ContinueWith(task => { });
                            }
                            await Task.Delay(TimeSpan.FromSeconds(gmSettings.RepetitionInterval), token).ContinueWith(task => { });
                        }
                    }, token);
                }
            }

            PlayingAlarmUpdated?.Invoke();
        }

        public void UpdatePlayingEquipmentAlarm(EquipmentType equipmentType)
        {
            if (!((EquipmentAlarmType == equipmentType) || (equipmentType == EquipmentType.GM0 && IsGm0AlarmMute) || (equipmentType == EquipmentType.GPS && IsGpsAlarmMute) || (equipmentType == EquipmentType.METEO && IsMeteoAlarmMute)))
            {
                StopAlarm();

                var gmSettings = _gmSettingsRepository.GetGMSettings();
                PlayingAlarmType = AlarmType.Equipment;
                EquipmentAlarmType = equipmentType;
                playingAlarmCancellationTokenSource = new CancellationTokenSource();
                var token = playingAlarmCancellationTokenSource.Token;
                string soundPath = string.Empty;

                switch (equipmentType)
                {
                    default:
                        {
                            soundPath = "ms-appx:///Sound/disconnected.wav";

                            break;
                        }
                }

                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        if ((equipmentType == EquipmentType.METEO && IsMeteoAlarmMute) || (equipmentType == EquipmentType.GM0 && IsGm0AlarmMute) || (equipmentType == EquipmentType.GPS && IsGpsAlarmMute)) { await Task.Delay(500, token).ContinueWith(task => { }); continue; }
                        var onMomentSoundCancellationTokenSource = new CancellationTokenSource();
                        this.onMomentSoundCancellationTokenSource = onMomentSoundCancellationTokenSource;
                        using (var player = new MediaPlayer())
                        {
                            player.Source = MediaSource.CreateFromUri(new Uri(soundPath));
                            player.Play();
                            await Task.Delay(TimeSpan.FromSeconds(2.8), onMomentSoundCancellationTokenSource.Token).ContinueWith(task => { });
                        }
                        await Task.Delay(TimeSpan.FromSeconds(gmSettings.RepetitionInterval), token).ContinueWith(task => { });
                    }
                }, token);
            }

            PlayingAlarmUpdated?.Invoke();
        }

        public void UpdateMuteDoseRateAlarm(bool isMute)
        {
            if (isMute)
            {
                doseRateAlarmMuteCancellationTokenSource?.Cancel();

                if (PlayingAlarmType == AlarmType.DoseRate) 
                {
                    onMomentSoundCancellationTokenSource?.Cancel();
                    onMomentSoundCancellationTokenSource = null;
                }

                var gmSettings = _gmSettingsRepository.GetGMSettings();
                IsDoseRateAlarmMute = true;
                doseRateAlarmMuteCancellationTokenSource = new CancellationTokenSource();
                var token = doseRateAlarmMuteCancellationTokenSource.Token;

                Task.Delay(TimeSpan.FromSeconds(gmSettings.BlackoutPeriod), token).ContinueWith(task =>
                {
                    IsDoseRateAlarmMute = false;
                    MuteAlarmStateChanged?.Invoke();
                });
            }
            else
            {
                doseRateAlarmMuteCancellationTokenSource?.Cancel();
                doseRateAlarmMuteCancellationTokenSource = null;
                IsDoseRateAlarmMute = false;
            }

            MuteAlarmStateChanged?.Invoke();
        }

        public void UpdateMuteEquipmentAlarm(bool isMute, EquipmentType equipmentType)
        {
            if (isMute)
            {
                if (EquipmentAlarmType == equipmentType) 
                {
                    onMomentSoundCancellationTokenSource?.Cancel();
                    onMomentSoundCancellationTokenSource = null;
                }
                var gmSettings = _gmSettingsRepository.GetGMSettings();

                switch (equipmentType)
                {
                    case EquipmentType.GM0:
                        {
                            gm0AlarmMuteCancellationTokenSource?.Cancel();

                            IsGm0AlarmMute = true;
                            gm0AlarmMuteCancellationTokenSource = new CancellationTokenSource();
                            var token = gm0AlarmMuteCancellationTokenSource.Token;

                            Task.Delay(TimeSpan.FromSeconds(gmSettings.BlackoutPeriod), token).ContinueWith(task =>
                            {
                                IsGm0AlarmMute = false;
                                MuteAlarmStateChanged?.Invoke();
                            });

                            break;
                        }
                    case EquipmentType.METEO:
                        {
                            meteoAlarmMuteCancellationTokenSource?.Cancel();

                            IsMeteoAlarmMute = true;
                            meteoAlarmMuteCancellationTokenSource = new CancellationTokenSource();
                            var token = meteoAlarmMuteCancellationTokenSource.Token;

                            Task.Delay(TimeSpan.FromSeconds(gmSettings.BlackoutPeriod), token).ContinueWith(task =>
                            {
                                IsMeteoAlarmMute = false;
                                MuteAlarmStateChanged?.Invoke();
                            });

                            break;
                        }
                    case EquipmentType.GPS:
                        {
                            gpsAlarmMuteCancellationTokenSource?.Cancel();

                            IsGpsAlarmMute = true;
                            gpsAlarmMuteCancellationTokenSource = new CancellationTokenSource();
                            var token = gpsAlarmMuteCancellationTokenSource.Token;

                            Task.Delay(TimeSpan.FromSeconds(gmSettings.BlackoutPeriod), token).ContinueWith(task =>
                            {
                                IsGpsAlarmMute = false;
                                MuteAlarmStateChanged?.Invoke();
                            });

                            break;
                        }
                }
            }
            else
            {
                switch (equipmentType)
                {
                    case EquipmentType.GM0:
                        {
                            gm0AlarmMuteCancellationTokenSource?.Cancel();
                            gm0AlarmMuteCancellationTokenSource = null;
                            IsGm0AlarmMute = false;

                            break;
                        }
                    case EquipmentType.GPS:
                        {
                            gpsAlarmMuteCancellationTokenSource?.Cancel();
                            gpsAlarmMuteCancellationTokenSource = null;
                            IsGpsAlarmMute = false;

                            break;
                        }
                    case EquipmentType.METEO:
                        {
                            meteoAlarmMuteCancellationTokenSource?.Cancel();
                            meteoAlarmMuteCancellationTokenSource = null;
                            IsMeteoAlarmMute = false;

                            break;
                        }
                }
            }

            MuteAlarmStateChanged?.Invoke();
        }

        public void StopAlarm()
        {
            onMomentSoundCancellationTokenSource?.Cancel();
            playingAlarmCancellationTokenSource?.Cancel();

            onMomentSoundCancellationTokenSource = null;
            playingAlarmCancellationTokenSource = null;
            
            EquipmentAlarmType = null;
            DoseRateAlarmLvl = null;
            PlayingAlarmType = null;
        }

        public void UpdateMuteAlarm(bool isMute)
        {
            UpdateMuteDoseRateAlarm(isMute);
            UpdateMuteEquipmentAlarm(isMute, EquipmentType.GPS);
            UpdateMuteEquipmentAlarm(isMute, EquipmentType.METEO);
            UpdateMuteEquipmentAlarm(isMute, EquipmentType.GM0);
        }
    }
}
