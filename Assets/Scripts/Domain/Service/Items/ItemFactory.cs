#nullable enable
using Domain.Model;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Service.Entities;
using UnityEngine;

namespace Domain.Service.Items
{
    public sealed class ItemFactory
    {
        public static ItemEntityMemento Build(Vector2Int spawnPosition, IItem item)
        {
            return new ItemEntityMemento
            {
                Item = item.Serialize(),
                Entity = Entity.Build(spawnPosition, EntityLayer.Bottom)
            };
        }

        public IItemEntity CreateItem(ItemEntityMemento item)
        {
            return new ItemEntity(item);
        }
    }
}