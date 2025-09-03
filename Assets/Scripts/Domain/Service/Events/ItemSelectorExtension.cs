#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Service.Events
{
    public static class ItemSelectorExtension
    {
        internal static async UniTask<IItem?> SelectItemWithCanSelect(this IItemSelector itemSelector, string text, IPlayer player, IMap map, Func<IItem, bool> canSelect)
        {
            var inventory = player.Character.Inventory;
            var disabledItemIndexes = new List<ItemFocus>();
            foreach (var (inventoryItem, index) in inventory.AllItemsWithIndexRecursive)
            {
                if (!canSelect(inventoryItem))
                {
                    disabledItemIndexes.Add(index);
                }
            }

            var groundItem = map.Items.At(player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
            if (groundItem != null && !canSelect(groundItem))
            {
                disabledItemIndexes.Add(ItemFocus.GroundItem);
            }

            var selectedItem = await itemSelector.SelectItem(text, inventory, map, disabledItemIndexes.ToArray());
            return selectedItem;
        }
    }
}