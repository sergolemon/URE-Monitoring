using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.Design;

namespace URE.Core.Models.Db
{
    public class MapTileDbContextFactory: IDesignTimeDbContextFactory<MapTileDbContext>
    {
        private readonly string _connectionString;

        public MapTileDbContextFactory() { }

        public MapTileDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public MapTileDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MapTileDbContext>();
            optionsBuilder.UseSqlite(_connectionString, x => x.MigrationsAssembly("URE.Core"));

            return new MapTileDbContext(optionsBuilder.Options);
        }
    }
}
