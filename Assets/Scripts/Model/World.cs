#nullable enable
using Model.Characters;
using Model.Characters.Behavior;
using Model.Items;
using Model.Setting;
using ObservableCollections;
using R3;
using RandomDungeonWithBluePrint;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;

namespace Model
{
    public class World
    {
        public readonly Character Player;
        public readonly List<MapManager> Maps = new();
        public int ActiveMapIndex = 0;
        public ReadOnlyReactiveProperty<MapManager> ActiveMap => _activeMap;
        private ReactiveProperty<MapManager> _activeMap = new();
        public readonly CharacterEvents PlayerEvents = new();
        public readonly CharacterEvents CharacterEvents = new();

        public IObservableCollection<Character> Characters => _characters;
        private readonly ObservableHashSet<Character> _characters = new();
        public IObservableCollection<ItemEntity> Items => _items;
        private readonly ObservableHashSet<ItemEntity> _items = new();
        private HashSet<Vector2Int> _allItemPositions = new();

        private readonly SerialDisposable[] _disposables = Enumerable.Range(0,2).Select(_ => new SerialDisposable()).ToArray();

        [Inject]
        public World(CharacterControllInputReceiver receiver)
        {
            Globals.World = this;

            _items.ObserveCountChanged().Subscribe(_ => SetAllItemPosition());

            Player = new CharacterFactory().CreatePlayer(Vector2Int.zero, receiver, Settings.IgnoreWall);
            PlayerEvents.Add(Player);
            FieldBluePrint bluePrint = Addressables.LoadAssetAsync<FieldBluePrint>("Assets/kyouma0220/RandomDungeonWithBluePrint/BluePrints/99_Random.asset").WaitForCompletion();
            GenerateMap(bluePrint);

            PlayerEvents.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                foreach (var item in _items)
                    if (areaChanged.AreaExited.Contains(item.CurrentPosition))
                        item.SetVisiblity(false);
                    else if (areaChanged.AreaEntered.Contains(item.CurrentPosition)) item.SetVisiblity(true);
            });

            CharacterEvents.OnPositionChanged.Subscribe(move =>
            {
                if (move.Character.Inventory.HasEmptySpace())
                {
                    var item = TryPickUp(move.Position);
                    if (item != null) move.Character.TryPickUp(item.Item);
                }
            });

            IsLoaded = true;
        }
        public void GenerateMap(FieldBluePrint bluePrint)
        {
            MapManager map = new(bluePrint, Player);
            Maps.Add(map);
            LoadMap(Maps.Count - 1);
            map.Spawn();
        }
        private void LoadMap(int index)
        {
            if (IsLoaded)
            {
                CharacterEvents.Remove(ActiveMap.CurrentValue.CharacterManager.CharacterEvents);
            }

            _activeMap.Value = Maps[index];

            CharacterEvents.Add(ActiveMap.CurrentValue.CharacterManager.CharacterEvents);

            _disposables[0].Disposable = _characters.SynchronizeWith(ActiveMap.CurrentValue.CharacterManager.Characters);
            _disposables[1].Disposable = _items.SynchronizeWith(ActiveMap.CurrentValue.ItemManager.Items);

            Player.Teleport(ActiveMap.CurrentValue.GetAllPassablePositions().GetAtRandom());
        }
        public ItemEntity? TryPickUp(Vector2Int position)
        {
            if (GetAllItemPositions().Contains(position))
            {
                var item = Items.First(item => item.CurrentPosition == position);
                _items.Remove(item);
                return item;
            }

            return null;
        }
        public HashSet<Vector2Int> GetAllItemPositions()
        {
            return _allItemPositions;
        }

        private void SetAllItemPosition()
        {
            _allItemPositions = Items.Select(item => item.CurrentPosition).ToHashSet();
        }
        public bool IsLoaded { get; private set; } = false;
    }
}