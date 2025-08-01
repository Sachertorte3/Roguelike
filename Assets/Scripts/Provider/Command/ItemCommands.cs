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
                var itemData = ScriptableObjectLoader.Load<ItemData>(itemName);
                var item = new Item(itemData);
                if (prefixName != null)
                {
                    var prefixData = ScriptableObjectLoader.Load<WeaponPrefix>(prefixName);
                    var itemMemento = WeaponFactory.Create(itemData, prefixData);
                    item = new Item(itemMemento);
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
    }
}