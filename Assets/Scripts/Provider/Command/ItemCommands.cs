#nullable enable
using System;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Service.Items;
using Game;
using IngameDebugConsole;
using Unity.Logging;
using Utilities;
using VContainer;

namespace Provider
{
    public class ItemCommands
    {
        private readonly World _world;

        [Inject]
        public ItemCommands(World world)
        {
            _world = world;

            DebugLogConsole.AddCommandInstance(
                "giveItem",
                "指定した対象のインベントリに指定したアイテムを追加します。",
                "GiveItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "giveItem",
                "指定した対象のインベントリに指定したアイテムを追加します。",
                "GivePrefixedItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "giveItemPlayer",
                "プレイヤーのインベントリに指定したアイテムを追加します。",
                "GiveItemPlayer",
                this);
            DebugLogConsole.AddCommandInstance(
                "giveItemPlayer",
                "プレイヤーのインベントリに指定したアイテムを追加します。",
                "GivePrefixedItemPlayer",
                this);
            DebugLogConsole.AddCommandInstance(
                "mergeItem",
                "プレイヤーインベントリ内の指定したアイテムを指定したアイテムと統合します。",
                "MergeItem",
                this);
        }

        private void GiveItemPlayer(string itemName)
        {
            GivePrefixedItem("player", itemName);
        }

        private void GivePrefixedItemPlayer(string itemName, string? prefixName = null)
        {
            GivePrefixedItem("player", itemName, prefixName);
        }

        private void GiveItem(string target, string itemName)
        {
            GivePrefixedItem(target, itemName);
        }

        private void GivePrefixedItem(string target, string itemName, string? prefixName = null)
        {
            try
            {
                var character = CommandUtilities.GetTarget(target, _world.ActiveMap.CurrentValue);
                var baseItemData = ScriptableObjectLoaderExtension.LoadItemData(itemName);
                var item = baseItemData.Match<IItem>(
                    itemData => new Item(itemData),
                    directWeaponData => new DirectWeapon(directWeaponData),
                    storageItemData => new StorageItem(storageItemData)
                );
                if (prefixName != null)
                {
                    var prefixData = ScriptableObjectLoader.Load<WeaponPrefix>(prefixName);
                    item = baseItemData.Match<IItem>(
                        itemData => throw new Exception($"Cannot add prefix {prefixName} to {itemName}"),
                        directWeaponData => new DirectWeapon(DirectWeapon.Build(directWeaponData, prefixData)),
                        storageItemData => new StorageItem(StorageItem.Build(storageItemData, prefixData))
                    );
                }

                if (character.Inventory.TryAdd(item))
                {
                    var map = _world.ActiveMap.CurrentValue;
                    Log.Info($"{item.GetName(map.Player, map.ItemPlaceholders)}を{target}のインベントリに追加しました。");
                }
                else
                {
                    Log.Info($"{target}のインベントリは一杯です。");
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void MergeItem(int index, int index2)
        {
            try
            {
                var inventory = _world.ActiveMap.CurrentValue.Player.Character.Inventory;
                var item = inventory.GetItem(index);
                var item2 = inventory.GetItem(index2);
                if (item == null || item2 == null)
                {
                    Log.Info($"インベントリ内の指定したアイテムが見つかりません。");
                    return;
                }
                if (!ItemMergeExtension.CanSelectForBaseItem(item) || !ItemMergeExtension.CanSelectForMergedItem(item2 as BaseItem, item))
                {
                    Log.Info($"指定したアイテムは合成できません。");
                    return;
                }
                var mergedItem = item.Merge(item2);
                inventory.Replace(mergedItem, index);
                inventory.Replace(null, index2);
                Log.Info($"{mergedItem.GetName(_world.ActiveMap.CurrentValue.Player, _world.ActiveMap.CurrentValue.ItemPlaceholders)}をプレイヤーのインベントリに追加しました。");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}