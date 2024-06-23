using Model.Domain.Items;
using UnityEngine;

namespace Model.Domain.Events
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