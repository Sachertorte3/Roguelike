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
using ObservableCollections;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using VContainer;

namespace Game
{
    public class Statistics : ISerializable<StatisticsMemento>
    {
        public TimeSpan LastSavePlayTime { get; private set; }
        public DateTime SessionStartTime { get; private set; }
        public TimeSpan CurrentSessionTime => DateTime.Now - SessionStartTime;
        public TimeSpan PlayTime => LastSavePlayTime + CurrentSessionTime;
        public ReactiveProperty<int> Turn { get; set; }
        public ObservableHashSet<string> KnownItemNames { get; private set; } = new();
        public Statistics(StatisticsMemento memento, GameManager game, World world)
        {
            LastSavePlayTime = TimeSpan.FromTicks(memento.PlayTime);
            SessionStartTime = DateTime.Now;
            Turn = new(memento.Turn);
            KnownItemNames = new(memento.KnownItemNames);

            world.ActiveMap.SubscribeToAllItemsIgnoreNull(map =>
            {
                map.Player.Character.KnownItemNames.ObserveChanged().Subscribe(item =>
                {
                    KnownItemNames.Add(item.NewItem);
                });
            });
            game.OnTurnChanged.Skip(1).Subscribe(_ =>
            {
                Turn.Value++;
            });
        }
        public StatisticsMemento Serialize()
        {
            return new StatisticsMemento(PlayTime.Ticks, Turn.Value, KnownItemNames.ToList());
        }
        public static StatisticsMemento Build()
        {
            return new StatisticsMemento(0, 0, new());
        }
    }
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
            await StopMap();
            bool isExistWorld = false;
            SaveData? saveData = null;
            if (_world.ActiveMap.CurrentValue == null)
            {
                saveData = _saveDataManager.Load(0);
                if (saveData != null)
                {
                    LoadWorld(saveData);
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
                            var world = _world.Serialize().RevivePlayer();
                            map = LoadWorld(saveData with { World = world });
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
            StartMap(map, saveData?.Statistics ?? Statistics.Build());
        }

        private MapManager CreateWorld()
        {
            Log.Debug("[Game]Start CreateWorld");
            _world.CreateNew();
            var map = _world.LoadMap(new Location("Dungeon", 1), null);
            Log.Debug("[Game]End CreateWorld");
            return map;
        }

        private MapManager LoadWorld(SaveData saveData)
        {
            return _world.LoadWorld(saveData.World, saveData.Maps);
        }

        private async UniTask StopMap()
        {
            _receiver.Enable(false);
            await _turnController.Stop();
        }

        private void StartMap(MapManager map, StatisticsMemento statistics)
        {
            _activeStatistics.Value = new Statistics(statistics, this, _world);
            _turnController.Run(this, map);
            _receiver.Enable(true);
        }

        public async void LoadMap(Location location, Id<IEntity>? destination = null)
        {
            Log.Debug("[Game]Start LoadMap");
            await StopMap();
            var map = _world.LoadMap(location, destination);
            Save();
            _turnController.Run(this, map);
            _receiver.Enable(true);
            Log.Debug("[Game]End LoadMap");
        }

        public void Save()
        {
            var world = _world.Serialize();
            var statistics = _activeStatistics.Value.Serialize();
            var maps = _world.SerializeUpdatedMaps().ToDictionary(map => map.Id.ToString(), map => map);
            _saveDataManager.Save(0, new SaveData(world, statistics, maps));
        }

        public async UniTask LoadAndStart()
        {
            await StopMap();
            var saveData = _saveDataManager.Load(0);
            MapManager map;
            if (saveData != null)
            {
                map = LoadWorld(saveData);
                StartMap(map, saveData.Statistics);
            }
            else
            {
                map = CreateWorld();
                StartMap(map, Statistics.Build());
            }
        }
    }
}