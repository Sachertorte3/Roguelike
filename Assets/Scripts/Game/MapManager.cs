#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Service.Characters;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Domain.Service.Logs;
using Domain.Service.Map;
using ObservableCollections;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Utilities.Algorithms;
using static Domain.Model.DungeonData;
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
        private Shop? _shop;
        public IShop? Shop => _shop;
        public ReadOnlyReactiveProperty<bool>? IsStolen => _shop?.IsStolen;
        public RectInt? ShopRect => _shop?.Rect;

        public MapManager(MapMemento map, SectionData sectionData, CharacterMemento? playerData, List<CharacterMemento>? partyMembers,
            Vector2Int playerPosition, CharacterControllInputReceiver receiver)
        {
            _tilemap = new Tilemap(map.Tilemap);
            CharacterManager = new CharacterManager();
            ItemManager = new ItemManager();
            EventEntityManager = new EventEntityManager(map.EventEntities);

            _sectionData = sectionData;

            if (playerData == null)
            {
                playerData = CharacterFactory.BuildPlayer("Player", playerPosition);
            }
            else
            {
                playerData = playerData with
                {
                    EntityData = playerData.EntityData with { Position = playerPosition }
                };
            }

            CharacterManager.SpawnPlayer(playerData, receiver, this);

            if (partyMembers != null)
            {
                foreach (var character in partyMembers)
                {
                    var characterData = character with
                    {
                        EntityData = character.EntityData with
                        {
                            Position = FindBlankPositionFrom(playerPosition,
                                position => !GetAllCharacterPositions().Contains(position))
                        }
                    };
                    CharacterManager.SpawnCharacter(characterData, this);
                }
            }

            foreach (var character in map.Characters)
            {
                CharacterManager.SpawnCharacter(character, this);
            }

            foreach (var item in map.Items)
            {
                ItemManager.SpawnItem(item);
            }

            SetRules();

            if (map.MonsterHouse != null)
            {
                _monsterHouse = new MonsterHouse(map.MonsterHouse);
                _eventAreas.Add(_monsterHouse);
            }

            if (map.Shop != null)
            {
                var clerk = Characters.FirstOrDefault(character => character.CurrentPosition == map.Shop.Clerk.Position);
                if (clerk == null && !map.Shop.IsStolen)
                    clerk = CharacterManager.SpawnCharacter(CharacterFactory.BuildCharacter(_sectionData.Clerk, map.Shop.Room.Room.RectRange().Where(IsPassable).GetAtRandom(), false, false), this);
                if (clerk != null)
                {
                    _shop = new Shop(map.Shop, clerk, this);
                    EventEntityManager.Add(_shop.Clerk);
                    _eventAreas.Add(_shop);
                }
            }

            var visibleArea = Player.VisionRange.VisibleArea;
            _tilemap.SetTilesKnown(visibleArea, true);

            foreach (var entity in Entities)
                entity.SetVisiblity(visibleArea.Contains(entity.CurrentPosition));
        }

        public ICharacter Player => CharacterManager.Player;

        public CharacterManager CharacterManager { get; init; }
        public IObservableCollection<IEventEntity> EventEntities => EventEntityManager.EventEntities;
        public IObservableCollection<IIconEventEntity> EventEntitiesAndIcons => EventEntityManager.EventEntitiesAndIcons;
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

        public IReadOnlyCollection<Vector2Int> VisibleArea => Player.VisionRange.VisibleArea;
        public IObservableCollection<ICharacter> Characters => CharacterManager.Characters;
        public IObservableCollection<IItemEntity> Items => ItemManager.Items;
        public IEnumerable<IEntity> Entities
        {
            get
            {
                foreach (var character in Characters)
                    yield return character;

                foreach (var item in Items)
                    yield return item;

                foreach (var eventEntity in EventEntities)
                    yield return eventEntity;
            }
        }

        public IItemEntity SpawnItem(IItem item, Vector2Int position)
        {
            return ItemManager.SpawnItem(item, position);
        }
        public ICharacter SpawnEnemy(EnemyData enemy, Vector2Int position)
        {
            return CharacterManager.SpawnCharacter(CharacterFactory.BuildCharacter(enemy, position, Random.value < _sectionData.SleepChance, Random.value < _sectionData.ShineyChance), this);
        }
        public ICharacter SpawnRandomEnemy(Vector2Int position)
        {
            return SpawnEnemy(_sectionData.Enemies.GetRandomItem(), position);
        }

        /// <summary>
        ///     Generates and returns a list of characters currently located within the given positions.
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public HashSet<ICharacter> GetCharactersInArea(IEnumerable<Vector2Int> area)
        {
            return Characters.Where(character => area.Contains(character.Position.CurrentValue))
                .ToHashSet();
        }

        public HashSet<IItemEntity> GetItemsInArea(IEnumerable<Vector2Int> area)
        {
            return Items.Where(item => area.Contains(item.Position.CurrentValue))
                .ToHashSet();
        }

        public IItem? GetItemFromId(Id<IItem> id)
        {
            var itemEntity = ItemManager.Items.FirstOrDefault(item => item.Item.Id == id);
            var item = itemEntity?.Item ?? Player.Inventory.AllItems.FirstOrDefault(i => i.Id == id);
            return item;
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

        public bool IsTouchableEventEntityAt(Vector2Int position, EntityLayer layer)
        {
            return EventEntities
                .Where(eventEntity => eventEntity.Trigger == EventTrigger.Touch)
                .Where(eventEntity => eventEntity.CurrentPosition == position)
                .Where(eventEntity => eventEntity.Layer == layer)
                .Where(eventEntity => eventEntity.CanExecuteEvent)
                .Any();
        }

        public HashSet<Vector2Int> GetAllPassablePositions()
        {
            var result = TilemapViewer.GetAllPassablePositions();
            result.ExceptWith(GetAllCharacterPositions());
            return result;
        }

        public HashSet<Vector2Int> GetPassablePositionsInArea(IEnumerable<Vector2Int> area)
        {
            var result = TilemapViewer.GetAllPassablePositions();
            result.IntersectWith(area);
            return result;
        }

        public bool IsPassable(Vector2Int position)
        {
            return IsMapPassable(position) && !GetAllEntityPositionsAt(EntityLayer.Middle).Contains(position);
        }

        public HashSet<Vector2Int> GetAllLightPassablePositions()
        {
            return TilemapViewer.GetAllPassablePositions();
        }

        public bool IsMapPassable(Vector2Int position)
        {
            return TilemapViewer.IsPassable(position);
        }

        public bool IsReachable(Vector2Int from, Vector2Int to)
        {
            return true; //TODO: A*で実装
        }

        public void Touch(Vector2Int position)
        {
            var eventEntity = EventEntities
                .Where(eventEntity => eventEntity.Trigger == EventTrigger.Touch)
                .Where(eventEntity => eventEntity.CurrentPosition == position)
                .Where(eventEntity => eventEntity.CanExecuteEvent)
                .FirstOrDefault();
            if (eventEntity != null)
                eventEntity.DoEvent(Globals.GameManager, this);
            else
                Log.Info($"I tried touch position {position} event but there was no event there.");
        }

        public void RemoveEventEntity(Chest eventEntity)
        {
            EventEntityManager.Remove(eventEntity);
        }

        public ITilemapViewer TilemapViewer => _tilemap;

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
                _monsterHouse?.Serialize(),
                _shop?.Serialize()
            );
        }

        private void SetRules()
        {
            CharacterManager.Characters.ObserveCountChanged().Subscribe(_ => SetAllCharacterPosition())
                .AddTo(_disposables);
            ItemManager.Items.ObserveCountChanged().Subscribe(_ => SetAllItemPosition()).AddTo(_disposables);

            CharacterManager.PlayerEvents.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                _tilemap.SetTilesKnown(areaChanged.Message.NewArea, true);

                foreach (var entity in Entities)
                    entity.SetVisiblity(areaChanged.Message.NewArea.Contains(entity.CurrentPosition));
            }).AddTo(_disposables);

            CharacterManager.PlayerEvents.OnPositionChanged.Subscribe(positionChanged =>
            {
                var eventEntity = EventEntities
                    .Where(eventEntity => eventEntity.Trigger == EventTrigger.Tread)
                    .Where(eventEntity => eventEntity.CurrentPosition == positionChanged.Message.Position)
                    .Where(eventEntity => eventEntity.CanExecuteEvent)
                    .FirstOrDefault();
                if (eventEntity != null)
                {
                    eventEntity.DoEvent(Globals.GameManager, this);
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
                            GameLog.Add($"{Player.GetName(Player)}は<color=yellow>{item.Item.Name}</color>を拾った");
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

            _tilemap.OnTilesChanged.Subscribe(tileChanged =>
            {
                CharacterManager.Characters.ForEach(character => character.VisionRange.Refresh(this));
            }).AddTo(_disposables);
        }

        ~MapManager()
        {
            Dispose();
        }

        public void UpdateTurn(int turn)
        {
            if (turn % 20 == 0)
                SpawnRandomEnemy(GetAllPassablePositions().Except(Player.VisionRange.VisibleArea).GetAtRandom());
        }

        public void RemoveWalls(IEnumerable<Vector2Int> positions)
        {
            _tilemap.RemoveWalls(positions);
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
                        .Select(eventEntity => eventEntity.CurrentPosition)).Concat(
                    Items.Where(item => item.Layer == layer)
                        .Select(item => item.CurrentPosition));
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
                GameLog.Add($"{Player.GetName(Player)}は{item.Name}を捨てた.");
                var itemEntity = ItemManager.TryPickUp(Player.CurrentPosition);
                if (itemEntity != null)
                {
                    GameLog.Add($"{Player.GetName(Player)}は{itemEntity.Item.Name}を拾った");
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
        public IEnumerable<ICharacter> GetFollowingCharacters()
        {
            return CharacterManager.Characters
                .Where(character => character != Player)
                .Where(character => character.IsAlly(Player))
                .Where(character => character.IsVisible(Player.CurrentPosition));
        }

        public Vector2Int FindBlankPositionFrom(Vector2Int position, Func<Vector2Int, bool> isBlankFunc)
        {
            return BlankFinder.FindBlankPosition(isBlankFunc, TilemapViewer.IsPassable, position);
        }
    }
}