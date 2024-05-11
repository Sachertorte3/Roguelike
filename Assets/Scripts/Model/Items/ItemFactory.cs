#nullable enable
using UnityEngine;

namespace Scripts.Model.Items
{
    internal sealed class ItemFactory
    {
        public ItemEntity CreateItem(Vector2Int spawnPosition, Item item)
        {
            return new ItemEntity(spawnPosition, item);
        }
    }
}
