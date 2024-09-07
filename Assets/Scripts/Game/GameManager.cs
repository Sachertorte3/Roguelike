#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using VContainer;

namespace Model.Game
{
    public enum GameState
    {
        Title,
        Dungeon,
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
        private readonly ReactiveProperty<GameState> _state = new ReactiveProperty<GameState>(GameState.Title);
        public ReadOnlyReactiveProperty<GameState> State => _state;
        private readonly SerialDisposable _disposable = new();

        [Inject]
        public GameManager(World world, GameInput input, ChoiceReceiver choiceReceiver, CharacterControlInputReceiver receiver, DungeonBluePrintData dungeonBluePrintData)
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
                        map = await CreateMap();
                        StartMap(map);
                        break;
                }
            }
            else
            {
                var _ = await GetChoice(null, "New Game");
                _state.Value = GameState.Dungeon;
                map = await CreateMap();
                StartMap(map);
            }
            _disposable.Disposable = map.Player.OnDestroyed.Subscribe(async _ =>
            {
                await StopMap();
                Save();
                _state.Value = GameState.Title;
            });
        }

        public async UniTask<MapManager> CreateMap()
        {
            Log.Debug("Start CreateMap");
            await StopMap();
            _world.CreateNew(_dungeonBluePrintData);
            var map = _world.LoadMap(new Location("Dungeon", 1), null);
            Log.Debug("End CreateMap");
            return map;
        }

        public async UniTask<int> GetChoice(string? text, params string[] choices)
        {
            return await _choiceReceiver.GetChoice(text, choices);
        }

        public async void LoadMap(Location location, Id<IEntity> destination)
        {
            Log.Debug("Start LoadMap");
            await StopMap();
            var map = _world.LoadMap(location, destination);
            _turnController.Run(map);
            _receiver.Enable(true);
            Log.Debug("End LoadMap");
        }

        public void Save()
        {
            Log.Debug("Start Save");
            var saveData = _world.Serialize();
            Debug.Log($"saveData:");
            foreach (var map in saveData.Maps)
            {
                foreach (var character in map.Value.KeyCharacters)
                {
                    Debug.Log($"keyCharacter:{character}");
                }
            }
            var saveDataStr = JsonUtility.ToJson(saveData);
            System.IO.File.WriteAllText("Save/save.json", saveDataStr);
            Log.Debug("End Save");
        }

        public async UniTask<MapManager> Load()
        {
            Log.Debug("Start Load");
            await StopMap();
            MapManager map = null;
            if (System.IO.File.Exists("Save/save.json"))
            {
                var str = System.IO.File.ReadAllText("Save/save.json");
                var saveData = JsonUtility.FromJson<WorldMemento>(str);
                map = _world.LoadWorld(saveData);
            }
            else
            {
                _world.CreateNew(_dungeonBluePrintData);
                map = _world.LoadMap(new Location("Dungeon", 1), null);
            }
            Log.Debug("End Load");
            return map;
        }

        public void StartMap(MapManager map)
        {
            _turnController.Run(map);
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