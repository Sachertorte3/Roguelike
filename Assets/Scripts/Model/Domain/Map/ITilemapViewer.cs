using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Model.Domain.Map
{
    public interface ITilemapViewer
    {
        public Observable<(Vector2Int position, TileData tile)> OnChangeTile { get; }
        public RectInt Rect { get; }
        public bool IsPassable(Vector2Int position);
        public IEnumerable<(Vector2Int position, TileData tileData)> GetAllTiles();
        public HashSet<Vector2Int> GetAllPassablePositions();
    }
}