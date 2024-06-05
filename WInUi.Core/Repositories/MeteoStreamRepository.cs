using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Contracts.Repositories;
using URE.Core.Models.Db;
using URE.Core.Models.Meteo;

namespace URE.Core.Repositories
{
    public class MeteoStreamRepository : IMeteoStreamRepository
    {
        private MeteoDbContextFactory _dbContextFactory;
        private readonly ILogger<MeteoStreamRepository> _logger;

        public MeteoStreamRepository(ILogger<MeteoStreamRepository> logger,
                                     MeteoDbContextFactory dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<MeteoStream>> GetByDates(List<DateTime> dates, int count, int offset = 0)
        {
            if (!dates.Any()) return new();

            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);

            List<MeteoStream> streams = await dbContext.MeteoStreams.Where(ms => ms.DateStart.Date == dates[0].Date).OrderBy(x => x.DateStart).Skip(offset).Take(count).Include(x => x.User).ToListAsync();
            List<MeteoData> meteoData = await dbContext.MeteoData.Where(md => md.Date == dates[0].Date).ToListAsync();

            streams.ForEach(s =>
            {
                s.Data = meteoData.Where(md => md.MeteoStreamId == s.Id).ToList();
            });

            return streams;
        }

        public async Task<int> AddMeteoStreamAsync(MeteoStream stream)
        {
            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);

            await dbContext.MeteoStreams.AddAsync(stream);
            await dbContext.SaveChangesAsync();

            return stream.Id;
        }

        public async Task<int> AddMeteoStreamAsync(MeteoStream meteoStream, IEnumerable<MeteoData> data)
        {
            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);
            using var tr = await dbContext.Database.BeginTransactionAsync();

            try
            {
                await dbContext.MeteoStreams.AddAsync(meteoStream);
                await dbContext.SaveChangesAsync();
                foreach (MeteoData record in data)
                {
                    record.MeteoStreamId = meteoStream.Id;
                }

                await dbContext.MeteoData.AddRangeAsync(data);
                await dbContext.SaveChangesAsync();
                await tr.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during meteo strem creation.");
                await tr.RollbackAsync();
            }

            return meteoStream.Id;
        }

        public async Task CloseStream(int id, DateTime dateEnd)
        {
            using MeteoDbContext dbContext = _dbContextFactory.CreateDbContext(null);

            MeteoStream stream = await dbContext.MeteoStreams.FirstOrDefaultAsync(ms => ms.Id == id);
            if (stream != null)
            {
                stream.DateEnd = dateEnd;
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<int> GetStreamsCountByDates(List<DateTime> dates)
        {
            if (!dates.Any()) return 0;

            using var dbContext = _dbContextFactory.CreateDbContext(null);

            return await dbContext.MeteoStreams.Where(x => x.DateStart.Date == dates[0].Date).CountAsync();
        }
    }
}
