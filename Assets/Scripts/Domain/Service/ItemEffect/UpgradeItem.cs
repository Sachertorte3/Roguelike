using System;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using UnityEngine;

namespace Domain.Service.ItemEffect
{
    [Serializable]
    public class UpgradeItem : IItemEffect
    {
        [SerializeField] private string _filter = "";

        public bool CanApplyTo(IHasInventory actor, IItem item)
        {
            return item.CanAnyUpgrade(_filter);
        }

        public void Apply(IHasInventory actor, IItem item, ItemPlaceholders itemPlaceholders)
        {
            item.RandomUpgrade(actor, itemPlaceholders, _filter);
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