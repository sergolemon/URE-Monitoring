using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using URE.Core.Models.Map;

namespace URE.Core.Models.Db
{
    public class MapTileDbContext: DbContext
    {
        public DbSet<MapTile> MapTile { get; set; }

        public MapTileDbContext(DbContextOptions<MapTileDbContext> options) : base(options)
        {
        }
    }
}
