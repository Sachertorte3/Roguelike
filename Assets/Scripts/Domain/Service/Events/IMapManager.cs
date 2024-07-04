using System.Collections.Generic;
using Domain.Model.Items;
using UnityEngine;

namespace Domain.Service.Events
{
    public interface IMapManager
    {
        public ICharacter Player { get; }
        public HashSet<ICharacter> GetCharactersInArea(IEnumerable<Vector2Int> area);
        public HashSet<IItemEntity> GetItemsInArea(IEnumerable<Vector2Int> area);
        public IItemEntity SpawnItem(IItem item, Vector2Int position);
        public void SpawnRandomEnemy(Vector2Int position);
        public void RemoveEventEntity(Chest entity);
    }
}