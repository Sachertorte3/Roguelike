#nullable enable
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Entity;
using Domain.Model.Map;
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
        private readonly ReactiveProperty<Statistics?> _activeStatistics = new();
        public ReadOnlyReactiveProperty<Statistics?> ActiveStatistics => _activeStatistics;
        private readonly ReactiveProperty<GameState> _state = new();
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
                _disposable.Disposable = map.Player.Character.Entity.OnDestroyed
                    .Where(_ => State.CurrentValue == GameState.Dungeon)
                    .Subscribe(async _ =>
                {
                    await StopMap();
                    Save();
                    GameOver();
                });
            });

            var disposable = new SerialDisposable();
            _activeStatistics.SubscribeIncludingCurrentValueIgnoreNull(statistics =>
                disposable.Disposable = Settings.WorldSettings.EnableCheat.Value.Subscribe(value =>
                {
                    if (value)
                    {
                        statistics.IsCheating = true;
                    }
                })
            );
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
                var revivePlayer = false;
                LoadPreview(saveData);
                var firstWaitTime = saveData.TurnWaitTime;
                if (!saveData.World.IsPlayerDead)
                {
                    var choice = await GetChoice(null, "Continue", "New Game");
                    switch (choice)
                    {
                        case 0:
                            break;
                        case 1:
                            saveData = null;
                            firstWaitTime = 0;
                            break;
                    }
                }
                else if (Settings.WorldSettings.RetryOnDead.CurrentValue)
                {
                    var choice = await GetChoice(null, "Retry", "New Game");
                    switch (choice)
                    {
                        case 0:
                            revivePlayer = true;
                            break;
                        case 1:
                            saveData = null;
                            firstWaitTime = 0;
                            break;
                    }
                }
                else
                {
                    var _ = await GetChoice(null, "New Game");
                    saveData = null;
                    firstWaitTime = 0;
                }

                MapManager map;
                if (saveData == null)
                {
                    map = CreateSaveData();
                }
                else if (revivePlayer)
                {
                    map = LoadSaveDataAndRevivePlayer(saveData);
                }
                else
                {
                    map = LoadSaveData(saveData);
                }

                StartGame(map, firstWaitTime);
            }
            else
            {
                var map = CreateSaveData();
                var _ = await GetChoice(null, "New Game");
                StartGame(map, 0);
            }

            _state.Value = GameState.Dungeon;
        }

        private MapManager LoadPreview(SaveData saveData)
        {
            return _world.LoadWorld(saveData.World, saveData.Maps);
        }

        private MapManager CreateSaveData()
        {
            _activeStatistics.Value = new Statistics(Statistics.Build(), this, _world);
            Settings.WorldSettings.Reset();

            _world.CreateNew();
            return _world.LoadStartMap();
        }

        private MapManager LoadSaveData(SaveData saveData)
        {
            _activeStatistics.Value = new Statistics(saveData.Statistics, this, _world);
            Settings.SetValues(saveData.Settings);
            if (saveData.IsRollbacked)
            {
                Log.Info($"[Game]rollback detected");
                _activeStatistics.Value.IsCheating = true;
            }

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

        private void StartGame(MapManager map, float firstWaitTime)
        {
            StartMap(map, firstWaitTime);
        }

        private void StartMap(MapManager map, float firstWaitTime)
        {
            Save();
            _receiver.Enable(true);
            _turnController.Run(this, map, firstWaitTime);
        }

        private async UniTask StopGame()
        {
            await StopMap();
        }

        private async UniTask StopMap()
        {
            _receiver.Enable(false);
            await _turnController.Stop();
        }

        public async void MoveMap(Id<IMap> mapId, Id<IEntity>? destination = null)
        {
            Log.Debug("[Game]Start LoadMap");
            await StopMap();
            var map = _world.LoadMap(mapId, destination);
            Save();
            StartMap(map, 0);
            Log.Debug("[Game]End LoadMap");
        }

        public void SaveLight()
        {
            _saveDataManager.SaveLight(Turn.CurrentValue);
        }

        public void Save()
        {
            Log.Info("[Game]Save");
            var world = _world.Serialize();
            var maps = _world.SerializeUpdatedMaps().ToDictionary(map => map.Id, map => map);
            var statistics = _activeStatistics.Value.Serialize();
            var settings = Settings.GetValues();
            var saveData = new SaveData(world, maps, statistics, settings, _turnController.GetWaitTime(), false);
            _saveDataManager.SaveFull(saveData);
            Log.Info("[Game]End Save");
        }

        public void ReturnTitle()
        {
            _state.Value = GameState.Title;
        }

        public void GameOver()
        {
            _state.Value = GameState.Title;
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}