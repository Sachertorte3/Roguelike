using System;
using Domain.Model.Item;
using Domain.Service.Items;
using Domain.Service.Logs;
using IngameDebugConsole;
using Model.Game;
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
            DebugLogConsole.AddCommandInstance("log", "画面にログを出力します。", "Log", this);
            DebugLogConsole.AddCommandInstance(
                "give",
                "指定したアイテムを指定した対象のインベントリに追加します。",
                "GiveItem",
                this);
        }

        private void Test(string message)
        {
            Debug.Log(message);
        }

        private void Log(string log)
        {
            GameLog.Add(log);
        }

        private void GiveItem(string itemName)
        {
            try
            {
                var player = _world.ActiveMap.CurrentValue.Player;
                var itemData = Addressables.LoadAssetAsync<ItemData>($"Assets/Database/ItemData/{itemName}.asset").WaitForCompletion();
                var item = new Item(itemData);
                player.Inventory.TryAdd(item);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}