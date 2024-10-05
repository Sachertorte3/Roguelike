using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Item;
using UnityEngine;

namespace Domain.Service.ItemEffect
{
    [Serializable]
    public class UpgradeItem : IItemEffect
    {
        [SerializeField] private string _filter = "";

        public IEnumerable<int> GetDisabledItemIndexes(IInventory inventory)
        {
            var disabledItems = inventory.AllItems.Where(item => !item.CanUpgrade(_filter));
            return disabledItems.Select(item => inventory.GetItemIndex(item));
        }

        public void Apply(IItem item)
        {
            item.Upgrade(_filter);
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