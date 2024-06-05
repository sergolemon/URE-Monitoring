using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using URE.Core.Contracts.Repositories;
using URE.Core.Models.Db;
using URE.Core.Models.Equipment;

namespace URE.Core.Repositories
{
    public class MeteoSettingRepository : IMeteoSettingsRepository
    {
        private MeteoDbContextFactory _dbContextFactory;

        public MeteoSettingRepository(MeteoDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task AddOrUpdateMeteoSettingsAsync(MeteoStationSettings data, CancellationToken cancellationToken = default)
        {
            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);

            var existsData = GetMeteoSettingsOrNull();

            if (existsData != null)
            {
                dbContext.MeteoStationSettings.Update(data);
            }
            else
            {
                dbContext.MeteoStationSettings.Add(data);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
        }

        public MeteoStationSettings GetMeteoSettingsOrNull()
        {
            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);
            return dbContext.MeteoStationSettings.AsNoTracking().SingleOrDefault();
        }
    }
}
