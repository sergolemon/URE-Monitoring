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
    public class DetectorSettings
    {
        public int Id { get; set; }

        public int GmSettingsId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string Name { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public int EquipmentIdentifier { get; set; }

        [Required]
        public long SerialNumber { get; set; }

        [Required]
        public int Color { get; set; }

        [Required]
        [Column(TypeName = "float(32)")]
        public double NormalValue { get; set; }

        [Required]
        [Column(TypeName = "float(32)")]
        public double HighValue { get; set; }

        [Required]
        [Column(TypeName = "float(32)")]
        public double CriticalValue { get; set; }
    }
}
