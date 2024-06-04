#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Model.Domain;
using Model.Domain.Characters;
using Model.Domain.Characters.Behavior;
using Model.Domain.Items;
using Model.Domain.Map;
using ObservableCollections;
using RandomDungeonWithBluePrint;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using R3;
using Unity.Logging;
using System;
using Model.Domain.Logs;
using Data.Character;
using Data.Map;

namespace Model.Game
{
    public class MapManager : IDisposable, ISerializable<MapMemento>, IMapViewer, IMap
    {
        private readonly Tilemap _tilemap;
        public Character Player => CharacterManager.Player;
        public IObservableCollection<Vector2Int> VisibleArea => Player.Area.VisibleArea;
        private HashSet<Vector2Int> _allCharacterPositions = new();
        private HashSet<Vector2Int> _allItemPositions = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly UpStairs? _upStairs;
        private readonly DownStairs _downStairs;
        public MapManager(MapMemento map, CharacterMemento? playerData, Vector2Int? playerPosition, CharacterControllInputReceiver receiver)
        {
            _tilemap = new Tilemap(map.Tilemap);
            CharacterManager = new CharacterManager();
            ItemManager = new ItemManager();

            EventEntities = new ObservableHashSet<IEventEntity>();

            CharacterManager.Characters.ObserveCountChanged().Subscribe(_ => SetAllCharacterPosition()).AddTo(_disposables);
            ItemManager.Items.ObserveCountChanged().Subscribe(_ => SetAllItemPosition()).AddTo(_disposables);

            CharacterManager.PlayerEvents.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                _tilemap.SetTilesKnown(areaChanged.Message.AreaEntered, true);

                foreach (var character in Characters)
                    if (areaChanged.Message.AreaExited.Contains(character.Position.CurrentValue))
                        character.SetVisiblity(false);
                    else if (areaChanged.Message.AreaEntered.Contains(character.Position.CurrentValue))
                        character.SetVisiblity(true);
                foreach (var item in Items)
                    if (areaChanged.Message.AreaExited.Contains(item.CurrentPosition))
                        item.SetVisiblity(false);
                    else if (areaChanged.Message.AreaEntered.Contains(item.CurrentPosition))
                        item.SetVisiblity(true);
                foreach (var eventEntity in EventEntities)
                    if (areaChanged.Message.AreaExited.Contains(eventEntity.CurrentPosition))
                        eventEntity.SetVisiblity(false);
                    else if (areaChanged.Message.AreaEntered.Contains(eventEntity.CurrentPosition))
                        eventEntity.SetVisiblity(true);
            }).AddTo(_disposables);

            CharacterManager.PlayerEvents.OnPositionChanged.Subscribe(positionChanged =>
            {
                foreach (var eventEntity in EventEntities)
                {
                    if (positionChanged.Message.Position == eventEntity.CurrentPosition)
                    {
                        eventEntity.DoEvent();
                    }
                }
                
                if (positionChanged.Character.Inventory.HasEmptySpace())
                {
                    var item = ItemManager.TryPickUp(positionChanged.Message.Position);
                    if (item != null)
                    {
                        if (positionChanged.Character.TryPickUp(item.Item))
                        {
                            GameLog.Add($"{Player.Name}: {item.Item.Name}を拾った");
                        }
                        else
                        {
                            Log.Error("cannot pick up item");
                        }
                    }
                }
            }).AddTo(_disposables);

            CharacterManager.CharacterEvents.OnPositionChanged.Subscribe(positionChanged =>
            {
                SetAllCharacterPosition();

                positionChanged.Character.SetVisiblity(
                    Player.Area.VisibleArea.Contains(positionChanged.Message.Position));
            }).AddTo(_disposables);

            ItemManager.ItemEntityEvents.OnPositionChanged.Subscribe(positionChanged =>
            {
                SetAllItemPosition();

                positionChanged.Item.SetVisiblity(Player.Area.VisibleArea.Contains(positionChanged.Message.Position));
            }).AddTo(_disposables);

            if (playerData == null || playerPosition == null)
            {
                playerData = Character.BuildPlayer(_tilemap.GetAllPassablePositions().GetAtRandom());
            }
            else
            {
                playerData = playerData with { EntityData = playerData.EntityData with { Position = playerPosition.Value } };
            }
            CharacterManager.SpawnPlayer(playerData, receiver, this);
            foreach (var character in map.Characters)
            {
                CharacterManager.SpawnCharacter(character, this);
            }
            foreach (var item in map.Items)
            {
                ItemManager.SpawnItem(item);
            }
            _downStairs = new DownStairs(map.DownStairs);
            EventEntities.Add(_downStairs);
            if (map.UpStairs != null)
            {
                _upStairs = new UpStairs(map.UpStairs);
                EventEntities.Add(_upStairs);
            }

            var visibleArea = Player.Area.VisibleArea;
            _tilemap.SetTilesKnown(visibleArea, true);
            foreach (var character in CharacterManager.Characters)
                character.SetVisiblity(visibleArea.Contains(character.Position.CurrentValue));
            foreach (var item in ItemManager.Items)
                item.SetVisiblity(visibleArea.Contains(item.CurrentPosition));
            foreach (var eventEntity in EventEntities)
                eventEntity.SetVisiblity(visibleArea.Contains(eventEntity.CurrentPosition));
        }
        public static MapMemento Build(TilemapMemento tilemapData, int nextMapId, int? prevMapId)
        {
            var tilemap = new Tilemap(tilemapData);
            var characters = new List<CharacterMemento>();
            var items = new List<ItemEntityMemento>();

            var data = Addressables.LoadAssetAsync<DungeonData>("Assets/Database/Dungeon.asset").WaitForCompletion();
            foreach (var position in tilemap.GetAllPassablePositions().GetAtRandom(10))
                characters.Add(Character.BuildCharacter(data.Enemies.GetAtRandom(), position));
            foreach (var position in tilemap.GetAllPassablePositions().GetAtRandom(30))
                items.Add(ItemEntity.Build(position, new Item(data.Items.GetAtRandom())));
            
            var downStairs = DownStairs.Build(tilemap.GetAllPassablePositions().GetAtRandom(), nextMapId);
            var upStairs = prevMapId.HasValue? UpStairs.Build(tilemap.GetAllPassablePositions().GetAtRandom(), prevMapId.Value) : null;

            return new MapMemento(
                tilemap.Serialize(),
                characters,
                items,
                downStairs,
                upStairs
            );
        }
        ~MapManager()
        {
            Dispose();
        }
        public void Dispose()
        {
            CharacterManager.Dispose();
            ItemManager.Dispose();
            EventEntities.ForEach(eventEntity => eventEntity.Dispose());
            _disposables.Dispose();
            Debug.Log("MapManager Disposed");
        }
        public MapMemento Serialize()
        {
            return new MapMemento(
                _tilemap.Serialize(),
                CharacterManager.Characters.Where(character => character != Player).Select(character => character.Serialize()).ToList(),
                ItemManager.Items.Select(item => item.Serialize()).ToList(),
                _downStairs.Serialize(),
                _upStairs?.Serialize()
            );
        }

        public CharacterManager CharacterManager { get; init; }
        public IObservableCollection<Character> Characters => CharacterManager.Characters;
        public IObservableCollection<ItemEntity> Items => ItemManager.Items;
        public ItemManager ItemManager { get; init; }
        public ObservableHashSet<IEventEntity> EventEntities { get; init; }
        public ITilemapViewer Tilemap => _tilemap;

        public ItemEntity? TryPickUp(Vector2Int position)
        {
            return ItemManager.TryPickUp(position);
        }

        public ItemEntity SpawnItem(Item item, Vector2Int position)
        {
            return ItemManager.SpawnItem(item, position);
        }

        /// <summary>
        ///     Generates and returns a list of characters currently located within the given positions.
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public HashSet<Character> GetCharactersInArea(IEnumerable<Vector2Int> area)
        {
            return Characters.Where(character => area.Contains(character.Position.CurrentValue))
                .ToHashSet();
        }

        public HashSet<Vector2Int> GetAllItemPositions()
        {
            return _allItemPositions;
        }

        private void SetAllItemPosition()
        {
            _allItemPositions = Items.Select(item => item.CurrentPosition).ToHashSet();
        }

        public bool IsPassableIgnoreWall(Vector2Int position)
        {
            return !GetAllCharacterPositions().Contains(position);
        }

        public HashSet<Vector2Int> GetAllPassablePositions()
        {
            var result = Tilemap.GetAllPassablePositions();
            result.ExceptWith(GetAllCharacterPositions());
            return result;
        }

        public HashSet<Vector2Int> GetAllCharacterPositions()
        {
            return new HashSet<Vector2Int>(_allCharacterPositions);
        }

        private void SetAllCharacterPosition()
        {
            _allCharacterPositions = Characters.Select(character => character.Position.CurrentValue).ToHashSet();
        }

        public void HandleItemDrop(int inventoryIndex)
        {
            var item = Player.Inventory.GetItem(inventoryIndex);
            if (item != null)
            {
                GameLog.Add($"{Player.Name}: {item.Name}を捨てた.");
                var itemEntity = TryPickUp(Player.CurrentPosition);
                if (itemEntity != null)
                {
                    GameLog.Add($"{Player.Name}: {itemEntity.Item.Name}を拾った");
                }
                Player.ReplaceInventory(itemEntity?.Item, inventoryIndex);
                ItemManager.SpawnItem(item, Player.CurrentPosition);
            }
        }
        public bool IsPassable(Vector2Int position)
        {
            return IsMapPassable(position) && !GetAllCharacterPositions().Contains(position);
        }

        public HashSet<Vector2Int> GetAllLightPassablePositions()
        {
            return Tilemap.GetAllPassablePositions();
        }

        public bool IsMapPassable(Vector2Int position)
        {
            return Tilemap.IsPassable(position);
        }

        public bool IsReachable(Vector2Int from, Vector2Int to)
        {
            return true; //TODO: A*で実装
        }
    }
}