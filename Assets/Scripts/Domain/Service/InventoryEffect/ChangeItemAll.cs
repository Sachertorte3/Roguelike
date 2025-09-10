using System;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Service.Items;
using UnityEngine;

namespace Domain.Service.InventoryEffect
{
    [Serializable]
    public class ChangeItemAll : IInventoryEffect
    {
        [SerializeField] private ItemData _item;

        public void Apply(IPlayer player, IStorage storage, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            for (var i = 0; i < storage.Capacity; i++)
            {
                if (storage.GetItem(i) != null)
                    storage.Replace(new Item(_item), i);
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