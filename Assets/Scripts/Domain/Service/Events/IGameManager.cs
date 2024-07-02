using System.Collections.Generic;
using Domain.Service.Characters;
using Domain.Service.Items;
using UnityEngine;

namespace Domain.Service.Events
{
    public interface IGameManager
    {
        public void LoadMap(int destinationMapId);
    }

    public interface IMapManager
    {
        public Character Player { get; }
        public HashSet<Character> GetCharactersInArea(IEnumerable<Vector2Int> area);
        public HashSet<ItemEntity> GetItemsInArea(IEnumerable<Vector2Int> area);
        public ItemEntity SpawnItem(Item item, Vector2Int position);
        public void SpawnRandomEnemy(Vector2Int position);
        public void RemoveEventEntity(Chest entity);
    }
}