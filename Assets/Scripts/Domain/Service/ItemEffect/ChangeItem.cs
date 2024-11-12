using System;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Service.Items;
using Domain.Service.Logs;
using UnityEngine;

namespace Domain.Service.ItemEffect
{
    [Serializable]
    public class ChangeItem : IItemEffect
    {
        [SerializeField] private ItemData _item;

        public bool CanApplyTo(IPlayer player, IItem item)
        {
            return true;
        }

        public void Apply(IPlayer player, IItem item, ItemPlaceholders itemPlaceholders)
        {
            player.Character.Inventory.Replace(new Item(_item), player.Character.Inventory.GetItemIndex(item));
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