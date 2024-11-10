#nullable enable
using Domain.Model;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Memento;
using UnityEngine;

namespace Domain.Service.Items
{
    public sealed class ItemFactory
    {
        public static ItemEntityMemento Build(Vector2Int spawnPosition, ItemMemento item)
        {
            return new ItemEntityMemento
            (
                item,
                EntityBase.Build(spawnPosition, EntityLayer.Bottom)
            );
        }

        public IItemEntity CreateItem(ItemEntityMemento item)
        {
            return new ItemEntity(item);
        }
    }
}