#nullable enable
using System;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Service.Items;
using Domain.Service.Logs;
using Game;
using IngameDebugConsole;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace Provider
{
    public class DebugCommands
    {
        private readonly GameManager _gameManager;
        private readonly World _world;

        [Inject]
        public DebugCommands(GameManager gameManager, World world)
        {
            _gameManager = gameManager;
            _world = world;

            DebugLogConsole.AddCommandInstance("test", "テスト", "Test", this);
            DebugLogConsole.AddCommandInstance("log", "画面にログを出力します。", "AddLog", this);
            DebugLogConsole.AddCommandInstance(
                "FindCharacter",
                "指定した位置にいるキャラクターのIDを取得します。",
                "FindCharacter",
                this);
            DebugLogConsole.AddCommandInstance(
                "FindAllCharacters",
                "すべてのキャラクターのIDを取得します。",
                "FindAllCharacters",
                this);
            DebugLogConsole.AddCommandInstance(
                "GetCharacterJson",
                "指定した位置にいるキャラクターのJsonを取得します。",
                "GetCharacterJson",
                this);
            DebugLogConsole.AddCommandInstance(
                "GiveItem",
                "指定した対象のインベントリに指定したアイテムを追加します。",
                "GiveItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "GiveItem",
                "指定した対象のインベントリに指定したアイテムを追加します。",
                "GivePrefixedItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "GiveItemPlayer",
                "プレイヤーのインベントリに指定したアイテムを追加します。",
                "GiveItemPlayer",
                this);
            DebugLogConsole.AddCommandInstance(
                "GiveItemPlayer",
                "プレイヤーのインベントリに指定したアイテムを追加します。",
                "GivePrefixedItemPlayer",
                this);
            DebugLogConsole.AddCommandInstance(
                "SpawnItem",
                "指定した位置に指定したアイテムをスポーンします。",
                "SpawnItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "SpawnItem",
                "指定した位置に指定したアイテムをスポーンします。",
                "SpawnPrefixedItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "SpawnEnemy",
                "指定した位置に指定した敵をスポーンします。",
                "SpawnEnemy",
                this);
            DebugLogConsole.AddCommandInstance(
                "MoveLevelTo",
                "指定したマップに移動します。",
                "MoveLevelTo",
                this);
        }

        private void Test(string message)
        {
            Debug.Log(message);
        }

        private void AddLog(string log)
        {
            GameLog.Add(log);
        }

        private ICharacter GetTarget(string target)
        {
            if (target == "Player" || target == "player")
            {
                return _world.ActiveMap.CurrentValue.Player;
            }

            if (Guid.TryParse(target, out var guid))
            {
                var character =
                    _world.ActiveMap.CurrentValue.Characters.FirstOrDefault(character =>
                        character.Id.ToString() == guid.ToString());
                if (character == null)
                {
                    throw new Exception("指定されたキャラクターが見つかりません。");
                }

                return character;
            }

            throw new Exception("不正なキャラクター指定です。");
        }

        private void ShowCharacter(ICharacter character)
        {
            var info = $"{character.GetName(_world.ActiveMap.CurrentValue.Player, true)}\n"
                       + $"Id: {character.Id}\n"
                       + $"Position: {character.Position.CurrentValue}\n"
                       + $"CharacterType: {character.CharacterType.SubtypeName()}";
            Log.Info(info);
        }

        private void ShowCharacterJson(ICharacter character)
        {
            Log.Info(JsonUtility.ToJson(character.Serialize(), true));
        }

        private void FindCharacter(Vector2Int position)
        {
            var character =
                _world.ActiveMap.CurrentValue.Characters.FirstOrDefault(character =>
                    character.Position.CurrentValue == position);
            if (character != null)
            {
                ShowCharacter(character);
            }
            else
            {
                Log.Error("指定された位置にキャラクターが見つかりません。");
            }
        }

        private void FindAllCharacters()
        {
            var characters = _world.ActiveMap.CurrentValue.Characters;
            foreach (var character in characters)
            {
                ShowCharacter(character);
            }
        }

        private void GetCharacterJson(string target)
        {
            try
            {
                var character = GetTarget(target);
                ShowCharacterJson(character);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        private void GiveItemPlayer(string itemName) => GivePrefixedItem("player", itemName, null);
        private void GivePrefixedItemPlayer(string itemName, string? prefixName = null) => GivePrefixedItem("player", itemName, prefixName);
        private void GiveItem(string target, string itemName) => GivePrefixedItem(target, itemName, null);
        private void GivePrefixedItem(string target, string itemName, string? prefixName = null)
        {
            try
            {
                var character = GetTarget(target);
                var itemData = Addressables.LoadAssetAsync<ItemData>($"Assets/Database/ItemData/{itemName}.asset")
                    .WaitForCompletion();
                var item = new Item(itemData);
                if (prefixName != null)
                {
                    var prefixData = Addressables.LoadAssetAsync<WeaponPrefix>($"Assets/Database/WeaponPrefix/{prefixName}.asset")
                        .WaitForCompletion();
                    var itemMemento = WeaponFactory.Create(itemData, prefixData);
                    item = new Item(itemMemento);
                }
                if (character.Inventory.TryAdd(item))
                {
                    Log.Info($"{item.Name}を{target}のインベントリに追加しました。");
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

        private void SpawnItem(string itemName, Vector2Int position) => SpawnPrefixedItem(itemName, position, null);
        private void SpawnPrefixedItem(string itemName, Vector2Int position, string? prefixName = null)
        {
            try
            {
                var itemData = Addressables.LoadAssetAsync<ItemData>($"Assets/Database/ItemData/{itemName}.asset")
                    .WaitForCompletion();
                var item = new Item(itemData);
                if (prefixName != null)
                {
                    var prefixData = Addressables.LoadAssetAsync<WeaponPrefix>($"Assets/Database/WeaponPrefix/{prefixName}.asset")
                        .WaitForCompletion();
                    var itemMemento = WeaponFactory.Create(itemData, prefixData);
                    item = new Item(itemMemento);
                }
                var spawnedItem = _world.ActiveMap.CurrentValue.SpawnItem(item, position);
                Log.Info($"{spawnedItem.Item.Name}を{position}にスポーンしました。");
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
                var enemyData = Addressables.LoadAssetAsync<EnemyData>($"Assets/Database/EnemyData/{enemyName}.asset")
                    .WaitForCompletion();
                var enemy = _world.ActiveMap.CurrentValue.SpawnEnemy(enemyData, position, isSlept: isSlept, isShiny: isShiny);
                Log.Info($"{enemy.GetName(_world.ActiveMap.CurrentValue.Player, true)}を{position}にスポーンしました。");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        
        private void MoveLevelTo(string mapName, int level)
        {
            try
            {
                _gameManager.LoadMap(new Location(mapName, level), null);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}