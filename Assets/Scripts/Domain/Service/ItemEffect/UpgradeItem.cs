using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using UnityEngine;

namespace Domain.Service.ItemEffect
{
    [Serializable]
    public class UpgradeItem : IItemEffect
    {
        [SerializeField] private string _filter = "";

        public IEnumerable<int> GetDisabledItemIndexes(IHasInventory actor)
        {
            var disabledItems = actor.Inventory.AllItems.Where(item => !item.CanUpgrade(_filter));
            return disabledItems.Select(item => actor.Inventory.GetItemIndex(item));
        }

        public void Apply(IHasInventory actor, IItem item, ItemPlaceholders itemPlaceholders)
        {
            item.Upgrade(actor, itemPlaceholders, _filter);
        }

        public float EvaluatePrice()
        {
            return 1000;
        }

        public string Info()
        {
            return _filter != "" ? $"強化({_filter})" : "強化(ランダム)";
        }
    }
}