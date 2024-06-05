using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URE.Core.Models.Equipment
{
    public class MeteoStationSettings
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(6)")]
        public string SerialPortName { get; set; }

        public int BaudRate { get; set; }

        public bool TemperatureEnabled { get; set; }

        public bool HumidityEnabled { get; set; }

        public bool WindDirectionEnabled { get; set; }

        public bool WindSpeedEnabled { get; set; }

        public bool PreassureEnabled { get; set; }
        [Required]
        public int EquipmentIdentifier { get; set; }

        [Required]
        public long SerialNumber { get; set; }
        //public bool MoveSpeedEnabled { get; set; }
        //public double MinMoveSpeed { get; set; }
        //public double MaxMoveSpeed { get; set; }
        //public int MoveSpeedColor { get; set; }
        //public int GPSSettingsId { get; set; }
        //public GPSSettings GPSSettings { get; set; }
    }
}
