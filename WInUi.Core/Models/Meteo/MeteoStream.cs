using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Models.Identity;

namespace URE.Core.Models.Meteo
{
    public class MeteoStream
    {
        public int Id { get; set; }

        [Required]
        public DateTime DateStart { get; set; }

        [Required]
        public DateTime DateEnd { get; set; }
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        [Required]
        public bool Auto { get; set; }

        public ICollection<MeteoData> Data { get; set; } = new List<MeteoData>();
    }
}
