#nullable enable
using Model.Characters;
using Model.Characters.Behavior;
using Model.Items;
using Model.Setting;
using ObservableCollections;
using R3;
using RandomDungeonWithBluePrint;
using Sirenix.Utilities;
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

        public readonly IntegratedSet<Character> Characters = new();
        public readonly IntegratedSet<ItemEntity> Items = new();
        private HashSet<Vector2Int> _allItemPositions = new();

        public IReadOnlyCollection<Vector2Int> VisibleArea => _visibleArea;
        private readonly HashSet<Vector2Int> _visibleArea = new();

        [Inject]
        public World(CharacterControllInputReceiver receiver)
        {
            Globals.World = this;

            Characters.Set.ObserveCountChanged().Subscribe(_ => SetAllCharacterPosition());
            Items.Set.ObserveCountChanged().Subscribe(_ => SetAllItemPosition());

            PlayerEvents.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                foreach (var item in Items.Set)
                    if (areaChanged.Message.AreaExited.Contains(item.CurrentPosition))
                        item.SetVisiblity(false);
                    else if (areaChanged.Message.AreaEntered.Contains(item.CurrentPosition))
                        item.SetVisiblity(true);
                if (areaChanged.Message.AreaExited.Contains(ActiveMap.CurrentValue.Stairs.Position))
                    ActiveMap.CurrentValue.Stairs.SetVisiblity(false);
                else if (areaChanged.Message.AreaEntered.Contains(ActiveMap.CurrentValue.Stairs.Position))
                    ActiveMap.CurrentValue.Stairs.SetVisiblity(true);
            });

            CharacterEvents.OnPositionChanged.Subscribe(move =>
            {
                if (move.Character.Inventory.HasEmptySpace())
                {
                    var item = TryPickUp(move.Message.Position);
                    if (item != null) move.Character.TryPickUp(item.Item);
                }
            });

            CharacterEvents.OnPositionChanged.Subscribe(positionChanged =>
            {
                SetAllCharacterPosition();
                positionChanged.Character.SetVisiblity(Player.Area.VisibleArea.Contains(positionChanged.Message.Position));
            });

            Player = new CharacterFactory().CreatePlayer(Vector2Int.zero, receiver, Settings.IgnoreWall);
            Characters.Add(Player);
            PlayerEvents.Add(Player);
            CharacterEvents.Add(Player);
            _visibleArea.LiveSynchronizeWith(Player.Area.VisibleArea);

            FieldBluePrint bluePrint = Addressables.LoadAssetAsync<FieldBluePrint>("Assets/kyouma0220/RandomDungeonWithBluePrint/BluePrints/99_Random.asset").WaitForCompletion();
            GenerateMap(bluePrint);

            IsLoaded = true;
        }
        public void GenerateMap(FieldBluePrint bluePrint)
        {
            MapManager map = new(bluePrint, _visibleArea);
            Maps.Add(map);
            LoadMap(Maps.Count - 1);
            map.Spawn();
        }
        private void LoadMap(int index)
        {
            if (IsLoaded)
            {
                CharacterEvents.Remove(ActiveMap.CurrentValue.CharacterManager.CharacterEvents);

                Characters.UnRegister(ActiveMap.CurrentValue.CharacterManager.Characters);
                Items.UnRegister(ActiveMap.CurrentValue.ItemManager.Items);
            }

            _activeMap.Value = Maps[index];

            CharacterEvents.Add(ActiveMap.CurrentValue.CharacterManager.CharacterEvents);

            Characters.Register(ActiveMap.CurrentValue.CharacterManager.Characters);
            Items.Register(ActiveMap.CurrentValue.ItemManager.Items);

            Player.Teleport(GetAllPassablePositions().GetAtRandom());
        }
        public ItemEntity? TryPickUp(Vector2Int position) => ActiveMap.CurrentValue.ItemManager.TryPickUp(position);
        public HashSet<Vector2Int> GetAllItemPositions()
        {
            return _allItemPositions;
        }

        private void SetAllItemPosition()
        {
            _allItemPositions = Items.Set.Select(item => item.CurrentPosition).ToHashSet();
        }
        public bool IsPassable(Vector2Int position)
        {
            return ActiveMap.CurrentValue.Tilemap.IsPassable(position) && !GetAllCharacterPositions().Contains(position);
        }

        public bool IsPassableIgnoreWall(Vector2Int position)
        {
            return !GetAllCharacterPositions().Contains(position);
        }

        /// <summary>
        ///     Generates and returns a list of characters currently located within the given positions.
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public HashSet<Character> GetCharactersInArea(HashSet<Vector2Int> area)
        {
            return Characters.Set.Where(character => area.Contains(character.Position.CurrentValue))
                .ToHashSet();
        }
        public HashSet<Vector2Int> GetAllPassablePositions()
        {
            var result = ActiveMap.CurrentValue.Tilemap.GetAllPassablePositions();
            result.ExceptWith(GetAllCharacterPositions());
            return result;
        }

        private HashSet<Vector2Int> _allCharacterPositions = new();
        public HashSet<Vector2Int> GetAllCharacterPositions()
        {
            return new HashSet<Vector2Int>(_allCharacterPositions);
        }

        private void SetAllCharacterPosition()
        {
            _allCharacterPositions = Characters.Set.Select(character => character.Position.CurrentValue).ToHashSet();
        }
        public bool IsLoaded { get; private set; } = false;
    }
}