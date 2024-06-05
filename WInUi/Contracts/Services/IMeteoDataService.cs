using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using URE.Core.Models.Meteo;

namespace URE.Contracts.Services
{
    public interface IMeteoDataService
    {
        MeteoData GetMeteoRow();

        void Export(string path, List<MeteoStream> data);
        Task<string> Import(IReadOnlyList<StorageFile> files, ICollection<MeteoStream> outputStreams = null!);
    }
}
