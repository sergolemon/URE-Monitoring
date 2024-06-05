using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace URE.Core.Models.Meteo
{
    public class MeteoStreamMetadata
    {
        [Index(0)]
        public DateTime Date { get; set; }
        [Index(1)]
        public bool Auto { get; set; }
        [Index(2)]
        public string CarNumber { get; set; }
        [Index(3)]
        public string Name { get; set; }
        [Index(4)]
        public string Surname { get; set; }
        [Index(5)]
        public string Patronymic { get; set; }
        [Index(6)]
        public string Login { get; set; }
    }
}
