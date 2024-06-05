using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Models.Meteo;

namespace URE.Core.Contracts.Repositories
{
    public interface IMeteoStreamRepository
    { 
        Task<int> GetStreamsCountByDates(List<DateTime> dates);
        Task<List<MeteoStream>> GetByDates(List<DateTime> dates, int count, int offset = 0);
        Task<int> AddMeteoStreamAsync(MeteoStream meteoStream);
        Task<int> AddMeteoStreamAsync(MeteoStream meteoStream, IEnumerable<MeteoData> data);

        Task CloseStream(int id, DateTime dateEnd);
    }
}
