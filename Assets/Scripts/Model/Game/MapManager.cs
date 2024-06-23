#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Character;
using Data.Effect;
using Data.Map;
using Model.Domain;
using Model.Domain.Characters;
using Model.Domain.Characters.Behavior;
using Model.Domain.Events;
using Model.Domain.Items;
using Model.Domain.Logs;
using Model.Domain.Map;
using ObservableCollections;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Utilities.Algorithms;
using static Data.DungeonData;
using Random = UnityEngine.Random;

namespace Model.Game
{
    public class MapManager : IDisposable, ISerializable<MapMemento>, IMapViewer, IMap, IMapManager
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly Tilemap _tilemap;
        private HashSet<Vector2Int> _allCharacterPositions = new();
        private HashSet<Vector2Int> _allItemPositions = new();
        private SectionData _sectionData;
        private List<IEventArea> _eventAreas = new();
        private MonsterHouse? _monsterHouse;

        public MapManager(MapMemento map, SectionData sectionData, CharacterMemento? playerData, List<CharacterMemento>? partyMembers,
            Vector2Int? playerPosition, CharacterControllInputReceiver receiver)
        {
            _tilemap = new Tilemap(map.Tilemap);
            CharacterManager = new CharacterManager();
            ItemManager = new ItemManager();
            EventEntityManager = new EventEntityManager(map.EventEntities);

            _sectionData = sectionData;
            
            _monsterHouse = new MonsterHouse(map.MonsterHouse);
            _eventAreas.Add(_monsterHouse);

            SetRules();

            if (playerData == null || playerPosition == null)
            {
                playerData = Character.BuildPlayer(_tilemap.GetAllPassablePositions().GetAtRandom());
            }
            else
            {
                playerData = playerData with
                {
                    EntityData = playerData.EntityData with { Position = playerPosition.Value }
                };
            }

            CharacterManager.SpawnPlayer(playerData, receiver, this);
            foreach (var character in map.Characters)
            {
                CharacterManager.SpawnCharacter(character, this);
            }

            if (partyMembers != null)
            {
                foreach (var character in partyMembers)
                {
                    var characterData = character with
                    {
                        EntityData = character.EntityData with
                        {
                            Position = FindBlankPositionFrom(playerPosition.Value,
                                position => !GetAllCharacterPositions().Contains(position))
                        }
                    };
                    CharacterManager.SpawnCharacter(characterData, this);
                }
            }

            foreach (var item in map.Items)
            {
                ItemManager.SpawnItem(item);
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

        public Character Player => CharacterManager.Player;

        public CharacterManager CharacterManager { get; init; }
        public IObservableCollection<IEventEntity> EventEntities => EventEntityManager.EventEntities;
        public ItemManager ItemManager { get; init; }
        public EventEntityManager EventEntityManager { get; init; }

        public void Dispose()
        {
            CharacterManager.Dispose();
            ItemManager.Dispose();
            EventEntities.ForEach(eventEntity => eventEntity.Dispose());
            _disposables.Dispose();
            Debug.Log("MapManager Disposed");
        }

        public IObservableCollection<Vector2Int> VisibleArea => Player.Area.VisibleArea;
        public IObservableCollection<Character> Characters => CharacterManager.Characters;
        public IObservableCollection<ItemEntity> Items => ItemManager.Items;

        public ItemEntity SpawnItem(Item item, Vector2Int position)
        {
            return ItemManager.SpawnItem(item, position);
        }
        public void SpawnRandomEnemy(Vector2Int position)
        {
            CharacterManager.SpawnCharacter(Character.BuildCharacter(_sectionData.Enemies.GetRandomItem(), position), this);
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

        public IEnumerable<Vector2Int> GetAllyPositions(IHasAffiliation character)
        {
            return Characters.Where(c => c.IsAlly(character)).Select(c => c.CurrentPosition);
        }

        public IEnumerable<Vector2Int> GetNeutralPositions(IHasAffiliation character)
        {
            return Characters.Where(c => c.IsNeutral(character)).Select(c => c.CurrentPosition);
        }

        public IEnumerable<Vector2Int> GetEnemyPositions(IHasAffiliation character)
        {
            return Characters.Where(c => c.IsEnemy(character)).Select(c => c.CurrentPosition);
        }

        public bool IsEventEntityAt(Vector2Int position, EntityLayer layer)
        {
            return EventEntities.Any(eventEntity =>
                eventEntity.CurrentPosition == position && eventEntity.Layer == layer);
        }

        public HashSet<Vector2Int> GetAllPassablePositions()
        {
            var result = Tilemap.GetAllPassablePositions();
            result.ExceptWith(GetAllCharacterPositions());
            return result;
        }

        public bool IsPassable(Vector2Int position)
        {
            return IsMapPassable(position) && !GetAllEntityPositionsAt(EntityLayer.Middle).Contains(position);
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

        public void Touch(Vector2Int position)
        {
            var eventEntity = EventEntities.Where(eventEntity => eventEntity.Trigger == EventTrigger.Touch)
                .FirstOrDefault(eventEntity => eventEntity.CurrentPosition == position);
            if (eventEntity != null)
                eventEntity.DoEvent(Globals.GameManager, this);
        }

        public void RemoveEventEntity(Chest eventEntity)
        {
            EventEntityManager.Remove(eventEntity);
        }

        public ITilemapViewer Tilemap => _tilemap;

        public MapMemento Serialize()
        {
            var characters = Characters.ToList();
            characters.Remove(Player);
            characters.RemoveAll(character => GetFollowingCharacters().Contains(character));
            return new MapMemento(
                _tilemap.Serialize(),
                characters.Select(character => character.Serialize()).ToList(),
                ItemManager.Items.Select(item => item.Serialize()).ToList(),
                EventEntityManager.Serialize(),
                _monsterHouse?.Serialize()
            );
        }

        private void SetRules()
        {
            CharacterManager.Characters.ObserveCountChanged().Subscribe(_ => SetAllCharacterPosition())
                .AddTo(_disposables);
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
                foreach (var eventEntity in EventEntities.Where(
                             eventEntity => eventEntity.Trigger == EventTrigger.Tread))
                {
                    if (positionChanged.Message.Position == eventEntity.CurrentPosition)
                    {
                        eventEntity.DoEvent(Globals.GameManager, this);
                    }
                }

                foreach (var eventArea in _eventAreas)
                {
                    eventArea.UpdatePosition(Globals.GameManager, this, positionChanged.Message.Position);
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
                    Player.IsVisible(positionChanged.Message.Position)
                );
            }).AddTo(_disposables);

            ItemManager.ItemEntityEvents.OnPositionChanged.Subscribe(positionChanged =>
            {
                SetAllItemPosition();

                positionChanged.Item.SetVisiblity(Player.IsVisible(positionChanged.Message.Position));
            }).AddTo(_disposables);
        }

        public static MapMemento Build(TilemapMemento tilemapData, SectionData data, int nextMapId, int? prevMapId)
        {
            var tilemap = new Tilemap(tilemapData);
            var characters = new List<CharacterMemento>();
            var items = new List<ItemEntityMemento>();
            var chests = new List<ChestMemento>();

            foreach (var position in tilemap.GetAllPassablePositions().GetAtRandom(10))
                characters.Add(Character.BuildCharacter(data.Enemies.GetRandomItem(), position));
            foreach (var position in tilemap.GetAllPassablePositions().GetAtRandom(15))
                items.Add(ItemEntity.Build(position, new Item(data.Items.GetRandomItem())));
            foreach (var position in tilemap.GetAllPassablePositions().GetAtRandom(5))
            {
                var material = data.Materials.GetRandomItem();
                var mold = data.WeaponMolds.GetRandomItem();
                if (Random.value < data.PrefixChance)
                {
                    var prefix = data.WeaponPrefixes.GetRandomItem();
                    var weapon = WeaponFactory.Create(prefix, material, mold);
                    items.Add(ItemEntity.Build(position, new Item(weapon)));
                }
                else
                {
                    var weapon = WeaponFactory.Create(material, mold);
                    items.Add(ItemEntity.Build(position, new Item(weapon)));
                }
            }

            foreach (var position in tilemap.GetAllPassablePositions().GetAtRandom(1))
            {
                var material = data.Materials.GetRandomItem();
                var mold = data.WeaponMolds.GetRandomItem();
                var prefix = data.WeaponPrefixes.GetRandomItem();
                var weapon = WeaponFactory.Create(prefix, material, mold);
                chests.Add(Chest.Build(position, weapon));
            }

            var downStairs = DownStairs.Build(tilemap.GetAllPassablePositions().GetAtRandom(), nextMapId);
            var upStairs = prevMapId.HasValue
                ? UpStairs.Build(tilemap.GetAllPassablePositions().GetAtRandom(), prevMapId.Value)
                : null;

            var eventEntities = EventEntityManager.Build(downStairs, upStairs, chests);

            var monsterHouse = MonsterHouse.Build(tilemap.Rooms.GetAtRandom());

            return new MapMemento(
                tilemap.Serialize(),
                characters,
                items,
                eventEntities,
                monsterHouse
            );
        }

        ~MapManager()
        {
            Dispose();
        }

        public ItemEntity? TryPickUp(Vector2Int position)
        {
            return ItemManager.TryPickUp(position);
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

        public HashSet<Vector2Int> GetAllCharacterPositions()
        {
            return new HashSet<Vector2Int>(_allCharacterPositions);
        }

        public IEnumerable<Vector2Int> GetAllEntityPositionsAt(EntityLayer layer)
        {
            return Characters.Where(character => character.Layer == layer)
                .Select(character => character.CurrentPosition).Concat(
                    EventEntities.Where(eventEntity => eventEntity.Layer == layer)
                        .Select(eventEntity => eventEntity.CurrentPosition));
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

        /// <summary>
        ///     Gets a character that follows the player when moving from one map to another.
        ///     Does not include the players themselves.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Character> GetFollowingCharacters()
        {
            return CharacterManager.Characters.Where(character =>
                character.IsAlly(Player) && character.IsVisible(Player.CurrentPosition));
        }

        public Vector2Int FindBlankPositionFrom(Vector2Int position, Func<Vector2Int, bool> isBlankFunc)
        {
            return BlankFinder.FindBlankPosition(isBlankFunc, Tilemap.IsPassable, position);
        }
    }
}