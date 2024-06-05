using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace URE.Core.Models.Db
{
    public class MeteoDbContextFactory : IDesignTimeDbContextFactory<MeteoDbContext>
    {
        private readonly string _connectionString;

        public MeteoDbContextFactory() { }

        public MeteoDbContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public MeteoDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MeteoDbContext>();
            optionsBuilder.UseSqlServer(_connectionString, x => x.MigrationsAssembly("URE.Core"));

            return new MeteoDbContext(optionsBuilder.Options);
        }
    }
}
