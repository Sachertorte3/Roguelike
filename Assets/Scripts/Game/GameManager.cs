#nullable enable
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Entity;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Domain.Service.Logs;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using VContainer;

namespace Game
{
    public class GameManager : IGameManager
    {
        private readonly World _world;
        private readonly TurnController _turnController;
        private readonly SaveDataManager _saveDataManager;
        public Func<bool>? IsDash;
        public Func<bool>? IsNoMove;
        private readonly ChoiceReceiver _choiceReceiver;
        private readonly TextInputReceiver _textInputReceiver;
        private readonly CharacterControlInputReceiver _receiver;
        public Observable<Unit> OnTurnChanged => _turnController.OnTurnChanged;
        public ReadOnlyReactiveProperty<int> Turn => _turnController.TurnInLevel;
        private readonly ReactiveProperty<Statistics> _activeStatistics = new();
        public ReadOnlyReactiveProperty<Statistics> ActiveStatistics => _activeStatistics;
        private readonly ReactiveProperty<GameState> _state = new(GameState.Title);
        public ReadOnlyReactiveProperty<GameState> State => _state;
        private readonly SerialDisposable _disposable = new();

        [Inject]
        public GameManager(World world, GameInput input, ChoiceReceiver choiceReceiver,
            TextInputReceiver textInputReceiver,
            CharacterControlInputReceiver receiver)
        {
            _world = world;
            _turnController = new TurnController(input);
            _saveDataManager = new SaveDataManager(0);
            _choiceReceiver = choiceReceiver;
            _textInputReceiver = textInputReceiver;
            _receiver = receiver;
            Globals.GameManager = this;

            _world.ActiveMap.SubscribeIncludingCurrentValueIgnoreNull(map =>
            {
                _disposable.Disposable = map.Player.Character.Entity.OnDestroyed.Subscribe(async _ =>
                {
                    await StopMap();
                    Save();
                    _state.Value = GameState.Title;
                });
            });
        }

        public UniTask<int> GetChoice(string? text, params string[] choices)
        {
            return _choiceReceiver.GetChoice(text, choices);
        }

        public UniTask<string> GetTextInput()
        {
            return _textInputReceiver.GetTextInput();
        }

        public async UniTask Title()
        {
            GameLog.Clear();
            await StopGame();
            var saveData = _saveDataManager.Load();
            if (saveData != null)
            {
                var map = LoadSaveData(saveData);
                if (!saveData.World.IsPlayerDead)
                {
                    var choice = await GetChoice(null, "Continue", "New Game");
                    switch (choice)
                    {
                        case 0:
                            break;
                        case 1:
                            map = CreateSaveData();
                            break;
                    }
                }
                else if (Settings.RetryOnDead.Value)
                {
                    var choice = await GetChoice(null, "Retry", "New Game");
                    switch (choice)
                    {
                        case 0:
                            map = LoadSaveDataAndRevivePlayer(saveData);
                            break;
                        case 1:
                            map = CreateSaveData();
                            break;
                    }
                }
                else
                {
                    var _ = await GetChoice(null, "New Game");
                    map = CreateSaveData();
                }
                StartGame(map, saveData.Statistics);
            }
            else
            {
                var map = CreateSaveData();
                var _ = await GetChoice(null, "New Game");
                StartGame(map, Statistics.Build());
            }

            _state.Value = GameState.Dungeon;
        }

        private MapManager CreateSaveData()
        {
            Log.Debug("[Game]Start CreateWorld");
            _world.CreateNew();
            var map = _world.LoadMap(new Location("Dungeon", 1), null);
            Log.Debug("[Game]End CreateWorld");
            return map;
        }

        private MapManager LoadSaveData(SaveData saveData)
        {
            return _world.LoadWorld(saveData.World, saveData.Maps);
        }

        private MapManager LoadSaveDataAndRevivePlayer(SaveData saveData)
        {
            var world = saveData.World.RevivePlayer();
            var map = LoadSaveData(saveData with { World = world });
            var randomPosition = map.GetAllBlankAndStandablePositionsOn().GetAtRandom().Position;
            map.Player.Character.Entity.Teleport(randomPosition);
            map.Player.Character.RestoreToFullHealth();
            map.Player.Character.Turn(Direction8.Down);
            return map;
        }

        private void StartMap(MapManager map)
        {
            _receiver.Enable(true);
            _turnController.Run(this, map);
        }

        private async UniTask StopMap()
        {
            _receiver.Enable(false);
            await _turnController.Stop();
        }

        private void StartGame(MapManager map, StatisticsMemento statistics)
        {
            _activeStatistics.Value = new Statistics(statistics, this, _world);
            StartMap(map);
        }

        private async UniTask StopGame()
        {
            await StopMap();
        }

        public async void LoadMap(Location location, Id<IEntity>? destination = null)
        {
            Log.Debug("[Game]Start LoadMap");
            await StopMap();
            var map = _world.LoadMap(location, destination);
            Save();
            StartMap(map);
            Log.Debug("[Game]End LoadMap");
        }

        public void Save()
        {
            var world = _world.Serialize();
            var statistics = _activeStatistics.Value.Serialize();
            var maps = _world.SerializeUpdatedMaps().ToDictionary(map => map.Id.ToString(), map => map);
            _saveDataManager.Save(new SaveData(world, statistics, maps));
        }

        public async UniTask LoadAndStart()
        {
            await StopMap();
            var saveData = _saveDataManager.Load();
            MapManager map;
            if (saveData != null)
            {
                map = LoadSaveData(saveData);
                StartGame(map, saveData.Statistics);
            }
            else
            {
                map = CreateSaveData();
                StartGame(map, Statistics.Build());
            }
        }
    }
}