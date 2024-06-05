using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Contracts.Repositories;
using URE.Core.Models.Db;
using URE.Core.Models.Equipment;

namespace URE.Core.Repositories
{
    public class GPSSettingsRepository : IGPSSettingsRepository
    {
        private MeteoDbContextFactory _dbContextFactory;

        public GPSSettingsRepository(MeteoDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task AddOrUpdateGPSSettingsAsync(GPSSettings data, CancellationToken cancellationToken = default)
        {
            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);

            var existsData = GetGPSSettingsOrNull();

            if (existsData != null)
            {
                dbContext.GPSSettings.Update(data);
            }
            else 
            {
                dbContext.GPSSettings.Add(data);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
        }

        public GPSSettings GetGPSSettingsOrNull()
        {
            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);
            return dbContext.GPSSettings.AsNoTracking().SingleOrDefault();
        }
    }
}
