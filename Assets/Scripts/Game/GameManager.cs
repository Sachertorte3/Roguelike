#nullable enable
using System;
using System.Collections.Generic;
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
        public ReadOnlyReactiveProperty<int> Turn => _turnController.Turn;
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
            _saveDataManager = new SaveDataManager();
            _choiceReceiver = choiceReceiver;
            _textInputReceiver = textInputReceiver;
            _receiver = receiver;
            Globals.GameManager = this;

            _world.ActiveMap.SubscribeToAllItemsIgnoreNull(map =>
            {
                _disposable.Disposable = map.Player.Character.Entity.OnDestroyed.Subscribe(async _ =>
                {
                    await StopMap();
                    _saveDataManager.Save(_world);
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
            await StopMap();
            bool isExistWorld = false;
            if (_world.ActiveMap.CurrentValue == null)
            {
                var world = _saveDataManager.Load();
                if (world != null)
                {
                    LoadWorld(world);
                    isExistWorld = true;
                }
                else
                {
                    CreateWorld();
                }
            }
            else
            {
                isExistWorld = true;
            }
            var map = _world.ActiveMap.CurrentValue;

            if (isExistWorld)
            {
                if (map.Player.Character.CurrentHp > 0)
                {
                    var choice = await GetChoice(null, "Continue", "New Game");
                    switch (choice)
                    {
                        case 0:
                            break;
                        case 1:
                            _saveDataManager.ClearSave();
                            map = CreateWorld();
                            break;
                    }
                }
                else if (Settings.RetryOnDead.Value)
                {
                    var choice = await GetChoice(null, "Retry", "New Game");
                    switch (choice)
                    {
                        case 0:
                            await StopMap();
                            var world = _world.Serialize().RevivePlayer();
                            map = LoadWorld(world);
                            var randomPosition = map.GetAllBlankAndStandablePositionsOn().GetAtRandom().Position;
                            map.Player.Character.Entity.Teleport(randomPosition);
                            map.Player.Character.RestoreToFullHealth();
                            map.Player.Character.Turn(Direction8.Down);
                            break;
                        case 1:
                            _saveDataManager.ClearSave();
                            map = CreateWorld();
                            break;
                    }
                }
                else
                {
                    var _ = await GetChoice(null, "New Game");
                    _saveDataManager.ClearSave();
                    map = CreateWorld();
                }
            }
            else
            {
                var _ = await GetChoice(null, "New Game");
            }

            _state.Value = GameState.Dungeon;
            StartMap(map);
        }

        private MapManager CreateWorld()
        {
            Log.Debug("[Game]Start CreateWorld");
            _world.CreateNew();
            var map = _world.LoadMap(new Location("Dungeon", 1), null);
            Log.Debug("[Game]End CreateWorld");
            return map;
        }

        private MapManager LoadWorld(WorldMemento world)
        {
            var maps = new List<(string, MapMemento)>();
            foreach (var mapId in world.MapIds)
            {
                var mapData = _saveDataManager.LoadMap(mapId);
                maps.Add((mapId, mapData));
            }

            return _world.LoadWorld(world, maps);
        }

        private async UniTask StopMap()
        {
            _receiver.Enable(false);
            await _turnController.Stop();
        }

        private void StartMap(MapManager map)
        {
            _turnController.Run(this, map);
            _receiver.Enable(true);
        }

        public async void LoadMap(Location location, Id<IEntity>? destination = null)
        {
            Log.Debug("[Game]Start LoadMap");
            await StopMap();
            var map = _world.LoadMap(location, destination);
            _saveDataManager.Save(_world);
            _turnController.Run(this, map);
            _receiver.Enable(true);
            Log.Debug("[Game]End LoadMap");
        }

        public void Save() => _saveDataManager.Save(_world);

        public async UniTask LoadAndStart()
        {
            await StopMap();
            var world = _saveDataManager.Load();
            MapManager map;
            if (world != null)
            {
                map = LoadWorld(world);
            }
            else
            {
                map = CreateWorld();
            }
            StartMap(map);
        }
    }
}