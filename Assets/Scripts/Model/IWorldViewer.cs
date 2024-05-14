#nullable enable
using System.Collections.Generic;
using Model.Characters;
using Model.Items;
using Model.Map;
using UnityEngine;

namespace Model
{
    internal interface IWorldViewer
    {
        public ITilemapViewer Map { get; }
        public CharacterManager CharacterManager { get; }
        public ItemManager ItemManager { get; }
        public bool IsPassable(Vector2Int position);
        public bool IsPassableIgnoreWall(Vector2Int position);
        public HashSet<Character> GetCharactersInArea(HashSet<Vector2Int> area);
    }
}