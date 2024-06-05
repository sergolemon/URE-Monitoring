using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Models.Equipment;

namespace URE.Core.Contracts.Repositories
{
    public interface IGMSettingsRepository
    {
        public GMSettings GetGMSettings();
        public Task AddOrUpdateGMSettingsAsync(GMSettings newSettings, CancellationToken cancellationToken = default);
    }
}
