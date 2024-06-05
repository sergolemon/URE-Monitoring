using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URE.Core.Models.Map;

namespace URE.Core.Contracts.Repositories
{
    public interface IMapTileRepository
    {
        MapTile Get(int col, int row, int scale);

        void Add(int col, int row, int scale, byte[] tile);
    }
}
