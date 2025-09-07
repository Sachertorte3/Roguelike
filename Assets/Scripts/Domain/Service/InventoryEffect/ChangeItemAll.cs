using System;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Service.Items;
using UnityEngine;

namespace Domain.Service.InventoryEffect
{
    [Serializable]
    public class ChangeItem : IInventoryEffect
    {
        [SerializeField] private ItemData _item;

        public void Apply(IPlayer player, IStorage storage, ItemPlaceholders itemPlaceholders)
        {
            foreach (var item in storage.AllItems)
            {
                player.Character.Inventory.Replace(new Item(_item), player.Character.Inventory.GetItemIndex(item));
            }
        }

        public float EvaluatePrice()
        {
            return 100 * 5;
        }

        public string Info()
        {
            return $"変化({_item.name})";
        }
    }
}