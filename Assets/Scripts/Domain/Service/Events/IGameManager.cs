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
        public ItemEntity SpawnItem(Item item, Vector2Int position);
        public void SpawnRandomEnemy(Vector2Int position);
        public void RemoveEventEntity(Chest entity);
    }
}