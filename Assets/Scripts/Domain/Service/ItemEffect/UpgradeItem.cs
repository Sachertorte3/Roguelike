using System;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;
using UnityEngine;

namespace Domain.Service.ItemEffect
{
    [Serializable]
    public class UpgradeItem : IItemEffect
    {
        [SerializeField] private string _filter = "";

        public bool CanApplyTo(IPlayer player, IItem item)
        {
            return item.CanUpgrade(_filter);
        }

        public void Apply(IPlayer player, IItem item, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            item.RandomUpgrade(player, itemHolder, itemPlaceholders, _filter);
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