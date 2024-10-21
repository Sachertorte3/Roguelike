using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Service.Items;
using UnityEngine;

namespace Domain.Service.ItemEffect
{
    [Serializable]
    public class ChangeItem : IItemEffect
    {
        [SerializeField] private ItemData _item;

        public IEnumerable<int> GetDisabledItemIndexes(IHasInventory actor)
        {
            return Enumerable.Empty<int>();
        }

        public void Apply(IHasInventory actor, IItem item, ItemPlaceholders itemPlaceholders)
        {
            actor.Inventory.Replace(new Item(_item), actor.Inventory.GetItemIndex(item));
        }

        public float EvaluatePrice()
        {
            return 100;
        }

        public string Info()
        {
            return $"変化({_item.name})";
        }
    }
}