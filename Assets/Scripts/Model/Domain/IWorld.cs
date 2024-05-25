using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data;
using Model.Characters;
using Model.Items;
using UnityEngine;

namespace Model.Domain
{
    public interface IWorld
    {
        public bool IsLoaded { get; }
        public HashSet<Character> GetCharactersInArea(HashSet<Vector2Int> area);
        public HashSet<Vector2Int> GetAllLightPassablePositions();
        public bool IsPassable(Vector2Int position);
        public bool IsMapPassable(Vector2Int position);
        public bool IsReachable(Vector2Int from, Vector2Int to);
        public ItemEntity SpawnItem(Item item, Vector2Int position);
    }
    public interface IInput
    {
        public bool IsDash();
        public bool IsNoMove();
    }
}
