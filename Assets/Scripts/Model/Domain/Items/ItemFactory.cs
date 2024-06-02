#nullable enable
using Data.Character;
using Data.Map;
using UnityEngine;

namespace Model.Domain.Items
{
    public sealed class ItemFactory
    {
        public ItemEntity CreateItem(ItemEntityMemento item)
        {
            return new ItemEntity(item);
        }
    }
}

