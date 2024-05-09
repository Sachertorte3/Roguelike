#nullable enable
using Scripts.Model.Characters;
using Scripts.Model.Map;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Model
{
    public interface IWorldViewer
    {
        public ITilemapViewer Map { get; }
        public bool IsPassable(Vector2Int position);
        public bool IsPassableIgnoreWall(Vector2Int position);
        public HashSet<Character> GetCharactersInArea(HashSet<Vector2Int> area);
    }
}
