using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Contracts.Repositories;
using URE.Core.Models.Db;
using URE.Core.Models.Meteo;

namespace URE.Core.Repositories
{
    public class MeteoDataRepository : IMeteoDataRepository
    {
        private MeteoDbContextFactory _dbContextFactory;

        public MeteoDataRepository(MeteoDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<MeteoData> GetLastByStreamId(int id)
        {
            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);

            return await dbContext.MeteoData
                .Where(m => m.MeteoStreamId == id)
                .OrderBy(m => m.Date)
                .ThenBy(m => m.Time)
                .LastOrDefaultAsync();
        }

        public async Task<List<MeteoData>> GetByPeriod(DateTime from, DateTime to)
        {
            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);

            List<MeteoData> meteoData = await dbContext.MeteoData.Where(ms => ms.Date >= from && ms.Date <= to).ToListAsync();
            return meteoData;
        }

        public async Task AddMeteoDataAsync(MeteoData data)
        {
            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);

            await dbContext.MeteoData.AddAsync(data);
            await dbContext.SaveChangesAsync();
        }

        public async Task AddMeteoDataAsync(IEnumerable<MeteoData> data)
        {
            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);

            await dbContext.MeteoData.AddRangeAsync(data);
            await dbContext.SaveChangesAsync();
        }
    }
}
