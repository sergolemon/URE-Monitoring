using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CsvHelper.Configuration.Attributes;
using CsvIndex = CsvHelper.Configuration.Attributes.IndexAttribute;
using Microsoft.EntityFrameworkCore;

namespace URE.Core.Models.Meteo
{
    [Microsoft.EntityFrameworkCore.Index(nameof(Date))]
    public class MeteoData
    {
        [Ignore]
        public int Id { get; set; }
        [Ignore]
        public int MeteoStreamId { get; set; }

        [Required]
        [Column(TypeName = "float(32)")]
        [CsvIndex(0)]
        public double Direction { get; set; } // 40003-04

        [Column(TypeName = "float(32)")]
        [CsvIndex(1)]
        public double Speed { get; set; } // 40005-06

        [Required]
        [Column(TypeName = "float(32)")]
        [CsvIndex(2)]
        public double CorrectedDirection { get; set; } // 40007-08

        [Required]
        [Column(TypeName = "float(32)")]
        [CsvIndex(3)]
        public double CorrectedSpeed { get; set; } // 40009-10

        [Required]
        [Column(TypeName = "float(32)")]
        [CsvIndex(4)]
        public double Pressure { get; set; } // 40011-12

        [Required]
        [Column(TypeName = "float(32)")]
        [CsvIndex(5)]
        public double RelativeHumidity { get; set; } // 40013-14

        [Required]
        [Column(TypeName = "float(32)")]
        [CsvIndex(6)]
        public double Temperature { get; set; } // 40015-16

        [Required]
        [Column(TypeName = "float(32)")]
        [CsvIndex(7)]
        public double DewPoint { get; set; } // 40017-18

        [Required]
        [Column(TypeName = "float(32)")]
        [CsvIndex(8)]
        public double GPSLatitude { get; set; } // 40019-20

        [Required]
        [Column(TypeName = "float(32)")]
        [CsvIndex(9)]
        public double GPSLongitude { get; set; } // 40021-22

        [Required]
        [CsvIndex(10)]
        public double GPSHeight { get; set; } // 40023-24

        [Required]
        [CsvIndex(11)]
        public DateTime Date { get; set; } // 40025-32

        [Required]
        [CsvIndex(12)]
        public TimeSpan Time { get; set; } // 40033-40

        [Ignore]
        public double SupplyVoltage { get; set; } // 40041-42

        [Ignore]
        public int Status { get; set;}

        [Column(TypeName = "float(32)")]
        [CsvIndex(13)]
        public double? D1Radiation { get; set; }

        [Column(TypeName = "float(32)")]
        [CsvIndex(14)]
        public double? D2Radiation { get; set; }

        [Column(TypeName = "float(32)")]
        [CsvIndex(15)]
        public double? D3Radiation { get; set; }

        [Column(TypeName = "float(32)")]
        [CsvIndex(16)]
        public double? D4Radiation { get; set; }

        [Column(TypeName = "float(32)")]
        [CsvIndex(17)]
        public double? D5Radiation { get; set; }

        [Column(TypeName = "float(32)")]
        [CsvIndex(18)]
        public double? D6Radiation { get; set; }

        [Column(TypeName = "float(32)")]
        [CsvIndex(19)]
        public double ManualInputRadiation { get; set; }

        //public string Operator { get; set; } = "Іван";
        //public string Comment { get; set; }

        [Ignore]
        public string D1RadiationStr => D1Radiation.HasValue ? $"{Math.Round(D1Radiation.Value, 3).ToString("0.000")} {Units}" : string.Empty;

        [Ignore]
        public string D2RadiationStr => D2Radiation.HasValue ? $"{Math.Round(D2Radiation.Value, 3).ToString("0.000")} {Units}" : string.Empty;

        [Ignore]
        public string D3RadiationStr => D3Radiation.HasValue ? $"{Math.Round(D3Radiation.Value, 3).ToString("0.000")} {Units}" : string.Empty;

        [Ignore]
        public string D4RadiationStr => D4Radiation.HasValue ? $"{Math.Round(D4Radiation.Value, 3).ToString("0.000")} {Units}" : string.Empty;

        [Ignore]
        public string D5RadiationStr => D5Radiation.HasValue ? $"{Math.Round(D5Radiation.Value, 3).ToString("0.000")} {Units}" : string.Empty;

        [Ignore]
        public string D6RadiationStr => D6Radiation.HasValue ? $"{Math.Round(D6Radiation.Value, 3).ToString("0.000")} {Units}" : string.Empty;
        [Ignore]
        public string ManualInputRadiationStr => $"{Math.Round(ManualInputRadiation, 3).ToString("0.000")} {Units}";

        [Ignore]
        public string CoordinatesStr => $"{GPSLatitude},\n{GPSLongitude}";

        [Ignore]
        public string DateStr => Date.ToShortDateString();

        [Ignore]
        public string TimeStr => (Date.Date + Time).ToLongTimeString();

        [Ignore]
        public static string Units { get; set; } = "мкЗв/год";

        [Ignore]
        public double MaxDoseRate => Math.Round(new double[]
        {
            D1Radiation ?? 0,
            D2Radiation ?? 0,
            D3Radiation ?? 0,
            D4Radiation ?? 0,
            D5Radiation ?? 0,
            D6Radiation ?? 0,
            ManualInputRadiation
        }.Max(), 3);

        [Ignore]
        public string MaxDoseRateStr => $"{Math.Round(MaxDoseRate, 3)} {Units}";
    }
}
