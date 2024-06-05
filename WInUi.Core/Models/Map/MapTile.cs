using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URE.Core.Models.Map
{
    [Index(nameof(Col), nameof(Row), nameof(Scale))]
    public class MapTile
    {
        public int Id { get; set; }

        [Required]
        public int Col { get; set; }

        [Required]
        public int Row { get; set; }

        [Required]
        public int Scale { get; set; }

        [Required]
        public byte[] Tile { get; set; }
    }
}
