using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using URE.Core.Models.Meteo;
using Newtonsoft.Json.Linq;

namespace URE.Core.Models.Equipment
{
    public class GMSettings
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(6)")]
        public string SerialPortName { get; set; }
        [Required]
        public int BaudRate { get; set; }
        [Required]
        [Column(TypeName = "float(32)")]
        public double NormalValue { get; set; }

        [Required]
        [Column(TypeName = "float(32)")]
        public double HighValue { get; set; }

        [Required]
        [Column(TypeName = "float(32)")]
        public double CriticalValue { get; set; }

        [Required]
        public int Duration { get; set; }

        [Required]
        public int BlackoutPeriod { get; set; }

        [Required]
        public int RepetitionInterval { get; set; }

        public ICollection<DetectorSettings> DetectorSettings { get; set; } = new List<DetectorSettings>();

        public SettingsValueState GetValueState(double value, int? detectorId = null)
        {
            List<DetectorSettings> settings = DetectorSettings.ToList();

            double normal = detectorId == null ? NormalValue : settings[detectorId.Value].NormalValue;
            double high = detectorId == null ? HighValue : settings[detectorId.Value].HighValue;
            double critical = detectorId == null ? CriticalValue : settings[detectorId.Value].CriticalValue;

            SettingsValueState state = SettingsValueState.Low;
            if (value < normal)
            {
                state = SettingsValueState.Low;
            }
            else if (value >= normal && value < high)
            {
                state = SettingsValueState.Normal;
            }
            else if (value >= high && value < critical)
            {
                state = SettingsValueState.High;
            }
            else
            {
                state = SettingsValueState.Critical;
            }

            return state;
        }

        public SettingsValueState GetValueState(MeteoData data)
        {
            SettingsValueState state = SettingsValueState.Low;

            List<DetectorSettings> settings = DetectorSettings.ToList();
            List<double?> doses = new List<double?>
            {
                data.D1Radiation,
                data.D2Radiation,
                data.D3Radiation,
                data.D4Radiation,
                data.D5Radiation,
                data.D6Radiation
            };

            foreach(double? dose in doses)
            {
                if (dose.HasValue)
                {
                    int index = doses.IndexOf(dose);
                    DetectorSettings doseSettings = settings[index];
                    SettingsValueState detectorState = SettingsValueState.Low;

                    if (dose < doseSettings.NormalValue)
                    {
                        detectorState = SettingsValueState.Low;
                    }
                    else if (dose >= doseSettings.NormalValue && dose < doseSettings.HighValue)
                    {
                        detectorState = SettingsValueState.Normal;
                    }
                    else if (dose >= doseSettings.HighValue && dose < doseSettings.CriticalValue)
                    {
                        detectorState = SettingsValueState.High;
                    }
                    else
                    {
                        detectorState = SettingsValueState.Critical;
                    }

                    state = detectorState > state ? detectorState : state;
                }
            }

            return state;
        }
    }
}
