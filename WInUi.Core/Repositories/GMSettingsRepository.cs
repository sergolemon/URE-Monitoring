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
    public class GmSettingsRepository : IGMSettingsRepository
    {
        private MeteoDbContextFactory _dbContextFactory;

        public GmSettingsRepository(MeteoDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public GMSettings GetGMSettings()
        {
            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);
            return dbContext.GMSettings.Include(s => s.DetectorSettings).AsNoTracking().FirstOrDefault();
        }

        public async Task AddOrUpdateGMSettingsAsync(GMSettings newSettings, CancellationToken cancellationToken = default)
        {
            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);

            var currSettings = GetGMSettings();

            if (currSettings == null)
            {
                dbContext.GMSettings.Add(newSettings);
            }
            else
            {
                dbContext.GMSettings.Update(newSettings);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
        }
    }
}
