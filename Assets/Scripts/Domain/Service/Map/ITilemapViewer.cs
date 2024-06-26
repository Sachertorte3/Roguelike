using System.Collections.Generic;
using Domain.Model.Map;
using R3;
using UnityEngine;

namespace Domain.Service.Map
{
    public interface ITilemapViewer : ISerializable<TilemapMemento>
    {
        public Observable<(Vector2Int position, TileData tile)> OnTileChanged { get; }
        public Observable<(Vector2Int position, TileData tile)> OnTileKnownChanged { get; }
        public RectInt Rect { get; }
        public bool IsPassable(Vector2Int position);
        public IEnumerable<(Vector2Int position, TileData tileData)> GetAllTiles();
        public HashSet<Vector2Int> GetAllPassablePositions();
        public void SetTilesKnown(IEnumerable<Vector2Int> positions, bool isKnown);
    }
}