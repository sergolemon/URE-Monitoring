using Microsoft.EntityFrameworkCore;
using URE.Core.Contracts.Repositories;
using URE.Core.Models.Db;
using URE.Core.Models.Map;

namespace URE.Core.Repositories
{
    public class MapTileRepository : IMapTileRepository
    {
        private readonly MapTileDbContextFactory _dbContextFactory;

        public MapTileRepository(MapTileDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public MapTile Get(int col, int row, int scale)
        {
            using MapTileDbContext dbContext = _dbContextFactory.CreateDbContext(null);
            return dbContext.MapTile.FirstOrDefault(m => m.Col == col && m.Row == row && m.Scale == scale);
        }

        public void Add(int col, int row, int scale, byte[] tile)
        {
            using MapTileDbContext dbContext = _dbContextFactory.CreateDbContext(null);

            MapTile mapTile = Get(col, row, scale);
            if(mapTile != null)
            {
                mapTile.Tile = tile;
            }
            else
            {
                mapTile = new MapTile
                {
                    Col = col,
                    Row = row,
                    Scale = scale,
                    Tile = tile
                };

                dbContext.MapTile.Add(mapTile);
            }

            dbContext.SaveChanges();
        }
    }
}
