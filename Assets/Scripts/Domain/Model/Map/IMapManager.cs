#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Item;
using UnityEngine;
using Utilities;

namespace Domain.Model.Map
{
    public interface IMapManager
    {
        public ICharacter Player { get; }
        public HashSet<Vector2Int> GetPassablePositionsInArea(IEnumerable<Vector2Int> area);
        public HashSet<ICharacter> GetCharactersInArea(IEnumerable<Vector2Int> area);
        public HashSet<IItemEntity> GetItemsInArea(IEnumerable<Vector2Int> area);
        public IItem? GetItemFromId(Id<IItem> id);
        public IItemEntity SpawnItem(IItem item, Vector2Int position);
        public ICharacter SpawnRandomEnemy(Vector2Int position, bool? isShiny = null);
        public void RemoveEventEntity(IEventEntity entity);
    }
}