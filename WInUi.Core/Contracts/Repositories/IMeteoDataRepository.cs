using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Models.Meteo;

namespace URE.Core.Contracts.Repositories
{
    public interface IMeteoDataRepository
    {
        Task<MeteoData> GetLastByStreamId(int id);
        Task<List<MeteoData>> GetByPeriod(DateTime from, DateTime to);
        Task AddMeteoDataAsync(MeteoData meteoData);
        Task AddMeteoDataAsync(IEnumerable<MeteoData> data);
    }
}
