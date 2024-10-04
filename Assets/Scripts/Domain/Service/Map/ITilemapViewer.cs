using System.Collections.Generic;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;

namespace Domain.Service.Map
{
    public interface ITilemapViewer : ISerializable<TilemapMemento>
    {
        public Observable<IEnumerable<(Vector2Int Position, TileData Tile)>> OnTilesChanged { get; }
        public Observable<IEnumerable<(Vector2Int Position, bool IsGrass)>> OnGrassesChanged { get; }
        public Observable<IEnumerable<(Vector2Int Position, bool IsKnown)>> OnTilesKnownChanged { get; }
        public RectInt Rect { get; }
        public bool IsPassable(Vector2Int position);
        public Option<TileData> GetTile(Vector2Int position);
        public IEnumerable<(Vector2Int position, TileData tileData)> GetAllTiles();
        public IEnumerable<Vector2Int> GetAllGrasses();
        public HashSet<Vector2Int> GetAllPassablePositions();
        public void SetTilesKnown(IEnumerable<Vector2Int> positions, bool isKnown);
    }
}