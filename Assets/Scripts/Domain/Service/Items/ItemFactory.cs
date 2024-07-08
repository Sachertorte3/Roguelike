#nullable enable
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Items;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Service.Items
{
    public sealed class ItemFactory
    {
        public static ItemEntityMemento Build(Vector2Int spawnPosition, IItem item)
        {
            return new ItemEntityMemento(
                item.Serialize(),
                new EntityMemento(spawnPosition, EntityLayer.Bottom)
            );
        }

        public IItemEntity CreateItem(ItemEntityMemento item)
        {
            return new ItemEntity(item);
        }
    }
}