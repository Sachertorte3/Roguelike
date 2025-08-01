#nullable enable
using System;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Service.Items;
using Game;
using IngameDebugConsole;
using Unity.Logging;
using UnityEngine;
using Utilities;
using VContainer;

namespace Provider
{
    public class SpawnCommands
    {
        private readonly World _world;

        [Inject]
        public SpawnCommands(World world)
        {
            _world = world;

            DebugLogConsole.AddCommandInstance(
                "spawnItem",
                "指定した位置に指定したアイテムをスポーンします。",
                "SpawnItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "spawnItem",
                "指定した位置に指定したアイテムをスポーンします。",
                "SpawnPrefixedItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "spawnEnemy",
                "指定した位置に指定した敵をスポーンします。",
                "SpawnEnemy",
                this);
        }

        private void SpawnItem(string itemName, Vector2Int position)
        {
            SpawnPrefixedItem(itemName, position);
        }

        private void SpawnPrefixedItem(string itemName, Vector2Int position, string? prefixName = null)
        {
            try
            {
                var itemData = ScriptableObjectLoader.Load<ItemData>(itemName);
                var item = new Item(itemData);
                if (prefixName != null)
                {
                    var prefixData = ScriptableObjectLoader.Load<WeaponPrefix>(prefixName);
                    var itemMemento = WeaponFactory.Create(itemData, prefixData);
                    item = new Item(itemMemento);
                }

                var spawnedItem = _world.ActiveMap.CurrentValue.SpawnItem(item, position);
                var map = _world.ActiveMap.CurrentValue;
                Log.Info($"{spawnedItem.Item.GetName(map.Player, map.ItemPlaceholders)}を{position}にスポーンしました。");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void SpawnEnemy(string enemyName, Vector2Int position, bool isSlept = false, bool isShiny = false)
        {
            try
            {
                var enemyData = ScriptableObjectLoader.Load<EnemyData>(enemyName);
                var enemy = _world.ActiveMap.CurrentValue.SpawnEnemy(enemyData, position, isSlept: isSlept,
                    isShiny: isShiny);
                Log.Info($"{enemy.GetNameIgnoreVisibility(_world.ActiveMap.CurrentValue.Player)}を{position}にスポーンしました。");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}