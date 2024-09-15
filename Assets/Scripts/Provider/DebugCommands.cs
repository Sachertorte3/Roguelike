using System;
using Domain.Model.Item;
using Domain.Service.Items;
using Domain.Service.Logs;
using IngameDebugConsole;
using Model.Game;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Unity.Logging;
using VContainer;
using Domain.Model.Character;
using System.Linq;

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
            if (target == "player")
            {
                return _world.ActiveMap.CurrentValue.Player;
            }
            else if (Guid.TryParse(target, out var guid))
            {
                var character = _world.ActiveMap.CurrentValue.Characters.FirstOrDefault(character => character.Id.ToString() == guid.ToString());
                if (character == null)
                {
                    throw new Exception($"指定されたキャラクターが見つかりません。");
                }
                return character;
            }
            else
            {
                throw new Exception($"不正なキャラクター指定です。");
            }
        }

        private void ShowCharacter(ICharacter character)
        {
            var info = $"{character.GetName(_world.ActiveMap.CurrentValue.Player, true)}\n"
            +$"Id: {character.Id}\n"
            +$"Position: {character.Position.CurrentValue}\n"
            +$"CharacterType: {character.CharacterType.SubtypeName()}";
            Log.Info(info);
        }

        private void ShowCharacterJson(ICharacter character)
        {
            Log.Info(JsonUtility.ToJson(character.Serialize(), true));
        }

        private void FindCharacter(Vector2Int position)
        {
            var character = _world.ActiveMap.CurrentValue.Characters.FirstOrDefault(character => character.Position.CurrentValue == position);
            if (character != null)
            {
                ShowCharacter(character);
            }
            else
            {
                Log.Error($"指定された位置にキャラクターが見つかりません。");
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
            var character = GetTarget(target);
            if (character != null)
            {
                ShowCharacterJson(character);
            }
            else
            {
                Log.Error($"指定された位置にキャラクターが見つかりません。");
            }
        }

        private void GiveItem(string target, string itemName)
        {
            try
            {
                var character = GetTarget(target);
                var itemData = Addressables.LoadAssetAsync<ItemData>($"Assets/Database/ItemData/{itemName}.asset").WaitForCompletion();
                var item = new Item(itemData);
                character.Inventory.TryAdd(item);
                Log.Info($"{itemName}を{target}のインベントリに追加しました。");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

    }
}