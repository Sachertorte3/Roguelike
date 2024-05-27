#nullable enable
using System.Collections.Generic;
using System.Linq;
using Data.Setting;
using Model.Domain;
using Model.Domain.Characters;
using Model.Domain.Characters.Behavior;
using Model.Domain.Items;
using ObservableCollections;
using R3;
using RandomDungeonWithBluePrint;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;

namespace Model.Game
{
    public class World : IWorld
    {
        private readonly HashSet<Vector2Int> _visibleArea = new();
        public readonly CharacterEvents CharacterEvents = new();

        public readonly IntegratedSet<Character> Characters = new();
        public readonly IntegratedSet<IEventEntity> EventEntities = new();
        public readonly IntegratedSet<ItemEntity> Items = new();
        public readonly List<MapManager> Maps = new();
        public readonly Character Player;
        public readonly CharacterEvents PlayerEvents = new();
        private ReactiveProperty<MapManager> _activeMap = new();

        private HashSet<Vector2Int> _allCharacterPositions = new();
        private HashSet<Vector2Int> _allItemPositions = new();
        public int ActiveMapIndex = 0;

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
                foreach (var eventEntity in EventEntities.Set)
                    if (areaChanged.Message.AreaExited.Contains(eventEntity.CurrentPosition))
                        eventEntity.SetVisiblity(false);
                    else if (areaChanged.Message.AreaEntered.Contains(eventEntity.CurrentPosition))
                        eventEntity.SetVisiblity(true);
            });

            PlayerEvents.OnPositionChanged.Subscribe(move =>
            {
                if (!IsLoaded) return;
                foreach (var eventEntity in ActiveMap.CurrentValue.EventEntities)
                {
                    if (move.Message.Position == eventEntity.CurrentPosition)
                    {
                        eventEntity.DoEvent();
                    }
                }
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
                positionChanged.Character.SetVisiblity(
                    Player.Area.VisibleArea.Contains(positionChanged.Message.Position));
            });

            Player = new CharacterFactory().CreatePlayer(Vector2Int.zero, receiver, Settings.IgnoreWall, this);
            Characters.Add(Player);
            PlayerEvents.Add(Player);
            CharacterEvents.Add(Player);
            _visibleArea.LiveSynchronizeWith(Player.Area.VisibleArea);

            var bluePrint = Addressables
                .LoadAssetAsync<FieldBluePrint>(
                    "Assets/kyouma0220/RandomDungeonWithBluePrint/BluePrints/99_Random.asset").WaitForCompletion();
            GenerateMap(bluePrint);

            IsInitialized = true;
        }

        public ReadOnlyReactiveProperty<MapManager> ActiveMap => _activeMap;

        public IReadOnlyCollection<Vector2Int> VisibleArea => _visibleArea;

        public bool IsInitialized { get; private set; } = false;

        public bool IsPassable(Vector2Int position)
        {
            return IsMapPassable(position) && !GetAllCharacterPositions().Contains(position);
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

        public HashSet<Vector2Int> GetAllLightPassablePositions()
        {
            return ActiveMap.CurrentValue.Tilemap.GetAllPassablePositions();
        }

        public bool IsMapPassable(Vector2Int position)
        {
            return ActiveMap.CurrentValue.Tilemap.IsPassable(position);
        }

        public bool IsReachable(Vector2Int from, Vector2Int to)
        {
            return IsPassable(to); //TODO: A*で実装
        }

        public ItemEntity SpawnItem(Item item, Vector2Int position)
        {
            return ActiveMap.CurrentValue.ItemManager.SpawnItem(item, position);
        }

        public bool IsLoaded { get; private set; } = false;

        public void GenerateMap(FieldBluePrint bluePrint)
        {
            MapManager map = new(bluePrint, _visibleArea);
            Maps.Add(map);
            LoadMap(Maps.Count - 1);
            map.Spawn(this);
        }

        private void LoadMap(int index)
        {
            IsLoaded = false;
            if (IsInitialized)
            {
                CharacterEvents.Remove(ActiveMap.CurrentValue.CharacterManager.CharacterEvents);

                Characters.UnRegister(ActiveMap.CurrentValue.CharacterManager.Characters);
                Items.UnRegister(ActiveMap.CurrentValue.ItemManager.Items);
                EventEntities.UnRegister(ActiveMap.CurrentValue.EventEntities);
            }

            _activeMap.Value = Maps[index];

            CharacterEvents.Add(ActiveMap.CurrentValue.CharacterManager.CharacterEvents);

            Characters.Register(ActiveMap.CurrentValue.CharacterManager.Characters);
            Items.Register(ActiveMap.CurrentValue.ItemManager.Items);
            EventEntities.Register(ActiveMap.CurrentValue.EventEntities);

            Player.Teleport(GetAllPassablePositions().GetAtRandom());

            IsLoaded = true;
        }

        public ItemEntity? TryPickUp(Vector2Int position)
        {
            return ActiveMap.CurrentValue.ItemManager.TryPickUp(position);
        }

        public HashSet<Vector2Int> GetAllItemPositions()
        {
            return _allItemPositions;
        }

        private void SetAllItemPosition()
        {
            _allItemPositions = Items.Set.Select(item => item.CurrentPosition).ToHashSet();
        }

        public bool IsPassableIgnoreWall(Vector2Int position)
        {
            return !GetAllCharacterPositions().Contains(position);
        }

        public HashSet<Vector2Int> GetAllPassablePositions()
        {
            var result = ActiveMap.CurrentValue.Tilemap.GetAllPassablePositions();
            result.ExceptWith(GetAllCharacterPositions());
            return result;
        }

        public HashSet<Vector2Int> GetAllCharacterPositions()
        {
            return new HashSet<Vector2Int>(_allCharacterPositions);
        }

        private void SetAllCharacterPosition()
        {
            _allCharacterPositions = Characters.Set.Select(character => character.Position.CurrentValue).ToHashSet();
        }

        public void HandleItemDrop(int inventoryIndex)
        {
            var item = Player.Inventory.GetItem(inventoryIndex);
            if (item != null)
            {
                var itemEntity = TryPickUp(Player.CurrentPosition);
                Player.ReplaceInventory(itemEntity?.Item, inventoryIndex);
                ActiveMap.CurrentValue.ItemManager.SpawnItem(item, Player.CurrentPosition);
            }
        }
    }
}