using BruTile;
using BruTile.Cache;
using URE.Core.Contracts.Repositories;
using URE.Core.Models.Map;

namespace URE.Services
{
    public class MapTileCacheService : IPersistentCache<byte[]>
    {
        public readonly string _cachingStorageFolder;
        private readonly IMapTileRepository _mapTileRpository;

        public MapTileCacheService(IMapTileRepository mapTileRepository) 
        {
            _mapTileRpository = mapTileRepository;
        }

        public void Add(TileIndex index, byte[] tile)
        {
            _mapTileRpository.Add(index.Col, index.Row, index.Level, tile);
        }

        public byte[] Find(TileIndex index)
        {
            MapTile mapTile = _mapTileRpository.Get(index.Col, index.Row, index.Level);
            return mapTile?.Tile;
        }

        public void Remove(TileIndex index)
        {
            throw new NotImplementedException();
        }
    }
}
