#nullable enable
using UnityEngine;

namespace Model.Domain.Items
{
    public sealed class ItemFactory
    {
        public ItemEntity CreateItem(Vector2Int spawnPosition, Item item)
        {
            return new ItemEntity(spawnPosition, item);
        }
    }
}