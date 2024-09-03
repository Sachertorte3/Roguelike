#nullable enable
using Domain.Model;
using Domain.Model.Memento;
using Domain.Service.Entities;
using UnityEngine;

namespace Domain.Service.Items
{
    public sealed class ItemFactory
    {
        public static ItemEntityMemento Build(Vector2Int spawnPosition, ItemMemento item)
        {
            return new ItemEntityMemento
            {
                Item = item,
                Entity = Entity.Build(spawnPosition, EntityLayer.Bottom)
            };
        }

        public IItemEntity CreateItem(ItemEntityMemento item)
        {
            return new ItemEntity(item);
        }
    }
}