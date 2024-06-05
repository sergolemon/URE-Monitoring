using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Models.Equipment;

namespace URE.Contracts.Services
{
    public interface IAlarmService
    {
        AlarmType? PlayingAlarmType { get; }
        void StopAlarm();
        void UpdateMuteAlarm(bool isMute);
        bool IsAlarmMute { get; }

        event Action MuteAlarmStateChanged;
        event Action PlayingAlarmUpdated;

        bool IsDoseRateAlarmMute { get; }
        SettingsValueState? DoseRateAlarmLvl { get; }
        void UpdatePlayingDoseRateAlarm(SettingsValueState alarmLvl);
        void UpdateMuteDoseRateAlarm(bool isMute);
        
        bool IsGpsAlarmMute { get; }
        bool IsMeteoAlarmMute { get; }
        bool IsGm0AlarmMute { get; }
        EquipmentType? EquipmentAlarmType { get; }
        void UpdatePlayingEquipmentAlarm(EquipmentType equipmentType);
        void UpdateMuteEquipmentAlarm(bool isMute, EquipmentType equipmentType);
    }

    public enum AlarmType
    {
        DoseRate,
        Equipment
    }
}
