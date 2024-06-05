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
    public class GPSSettings
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(6)")]
        public string SerialPortName { get; set; }

        public int BaudRate { get; set; }

        public bool MoveSpeedEnabled { get; set; }
        public double MinMoveSpeed { get; set; }
        public double MaxMoveSpeed { get; set; }
        public int MoveSpeedColor { get; set; }
        public bool HeightEnabled { get; set; }
        // public MeteoStationSettings MeteoStationSettings { get; set; }
    }
}
