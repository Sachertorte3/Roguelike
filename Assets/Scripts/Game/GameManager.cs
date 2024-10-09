#nullable enable
using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Dungeon;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Domain.Service.Logs;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using VContainer;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Game
{
    public enum GameState
    {
        Title,
        Dungeon
    }

    public class GameManager : IGameManager
    {
        private readonly World _world;
        private readonly TurnController _turnController;
        public Func<bool>? IsDash;
        public Func<bool>? IsNoMove;
        private readonly ChoiceReceiver _choiceReceiver;
        private readonly CharacterControlInputReceiver _receiver;
        private readonly DungeonBluePrintData _dungeonBluePrintData;
        public ReadOnlyReactiveProperty<int> Turn => _turnController.Turn;
        private readonly ReactiveProperty<GameState> _state = new(GameState.Title);
        public ReadOnlyReactiveProperty<GameState> State => _state;
        private readonly SerialDisposable _disposable = new();

        [Inject]
        public GameManager(World world, GameInput input, ChoiceReceiver choiceReceiver,
            CharacterControlInputReceiver receiver, DungeonBluePrintData dungeonBluePrintData)
        {
            _world = world;
            _turnController = new TurnController(input);
            _choiceReceiver = choiceReceiver;
            _receiver = receiver;
            _dungeonBluePrintData = dungeonBluePrintData;
            Globals.GameManager = this;
        }

        public async UniTask Title()
        {
            GameLog.Clear();
            if (_world.ActiveMap.CurrentValue == null)
            {
                await Load();
            }

            var map = _world.ActiveMap.CurrentValue;
            if (map.Player.CurrentHp > 0)
            {
                var choice = await GetChoice(null, "Continue", "New Game");
                _state.Value = GameState.Dungeon;
                switch (choice)
                {
                    case 0:
                        StartMap(map);
                        break;
                    case 1:
                        ClearSave();
                        map = await CreateWorld();
                        StartMap(map);
                        break;
                }
            }
            else
            {
                var _ = await GetChoice(null, "New Game");
                _state.Value = GameState.Dungeon;
                ClearSave();
                map = await CreateWorld();
                StartMap(map);
            }

            _world.ActiveMap.Subscribe(map =>
            {
                _disposable.Disposable = map.Player.OnDestroyed.Subscribe(async _ =>
                {
                    await StopMap();
                    Save();
                    _state.Value = GameState.Title;
                });
            });
        }

        public async UniTask<MapManager> CreateWorld()
        {
            Log.Debug("Start CreateWorld");
            await StopMap();
            _world.CreateNew(_dungeonBluePrintData);
            var map = _world.LoadMap(new Location("Dungeon", 1), null);
            Log.Debug("End CreateWorld");
            return map;
        }

        public async UniTask<int> GetChoice(string? text, params string[] choices)
        {
            return await _choiceReceiver.GetChoice(text, choices);
        }

        public async void LoadMap(Location location, Id<IEntity>? destination = null)
        {
            Log.Debug("Start LoadMap");
            await StopMap();
            var map = _world.LoadMap(location, destination);
            Save();
            _turnController.Run(this, map);
            _receiver.Enable(true);
            Log.Debug("End LoadMap");
        }

        public void Save()
        {
            Log.Debug("[Save]Start Save");
            var saveData = _world.Serialize();
            var maps = _world.SerializeUpdatedMaps();
            WriteData("Save/save.json", JsonUtility.ToJson(saveData));
            foreach (var map in maps)
            {
                Log.Debug($"[Save]Save map: {map.Id}");
                WriteData($"Save/{map.Id}.json", JsonUtility.ToJson(map));
            }
            Log.Debug("[Save]End Save");
        }

        public async UniTask<MapManager> Load()
        {
            Log.Debug("[Save]Start Load");
            await StopMap();
            MapManager map = null;
            var saveData = ReadData("Save/save.json");
            if (saveData != null)
            {
                var world = JsonUtility.FromJson<WorldMemento>(saveData);
                var maps = new List<(string, MapMemento)>();
                foreach (var mapId in world.MapIds)
                {
                    var mapData = JsonUtility.FromJson<MapMemento>(ReadData($"Save/{mapId}.json"));
                    maps.Add((mapId, mapData));
                }
                map = _world.LoadWorld(world, maps);
            }
            else
            {
                _world.CreateNew(_dungeonBluePrintData);
                map = _world.LoadMap(new Location("Dungeon", 1), null);
            }

            Log.Debug("[Save]End Load");
            return map;
        }

        public void ClearSave()
        {
            var saveDirectory = "Save";
            var jsonFiles = Directory.GetFiles(saveDirectory, "*.json");
            foreach (var file in jsonFiles)
            {
                File.Delete(file);
            }
        }

        public void WriteData(string path, string saveData)
        {
            if (saveData.Contains("❰") || saveData.Contains("❱"))
            {
                throw new Exception("Save data is corrupted");
            }
            saveData = Regex.Replace(saveData, @"<(.+?)>k__BackingField", "❰$1❱");
            File.WriteAllText(path, saveData);
        }

        public string? ReadData(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var saveDataStr = File.ReadAllText(path);
            saveDataStr = Regex.Replace(saveDataStr, @"❰(.+?)❱", "<$1>k__BackingField");
            return saveDataStr;
        }

        public void StartMap(MapManager map)
        {
            _turnController.Run(this, map);
            _receiver.Enable(true);
        }

        public async UniTask StopMap()
        {
            _receiver.Enable(false);
            await _turnController.Stop();
        }

        public async UniTask LoadAndStart()
        {
            var map = await Load();
            StartMap(map);
        }
    }
}