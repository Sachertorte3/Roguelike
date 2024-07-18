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
using Domain.Service.Entities;
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

            _tilemap = new Tilemap(map.Tilemap);
            CharacterManager = new CharacterManager(playerData, receiver, this);
            ItemManager = new ItemManager();
            EventEntityManager = new EventEntityManager(map.EventEntities);

            _sectionData = sectionData;

            if (partyMembers != null)
            {
                foreach (var character in partyMembers)
                {
                    var characterData = character with
                    {
                        EntityData = character.EntityData with
                        {
                            Position = FindBlankPositionFrom(playerPosition,
                                position => !AllCharacterPositions().Contains(position))
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

            var visibleArea = CharacterManager.Player.VisionRange.VisibleArea;
            _tilemap.SetTilesKnown(visibleArea, true);

            foreach (var entity in Entities)
                entity.SetVisiblity(visibleArea.Contains(entity.CurrentPosition));
        }

        public ICharacter? Player => CharacterManager?.Player;

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
        public ICharacter SpawnEnemy(EnemyData enemy, Vector2Int position, IAffiliation? affiliation = null, bool? isSleeped = null, bool? isShiney = null)
        {
            return CharacterManager.SpawnCharacter(
                CharacterFactory.BuildCharacter(
                    enemy,
                    position,
                    isSleeped ?? Random.value < _sectionData.SleepChance,
                    isShiney ?? Random.value < _sectionData.ShineyChance,
                    affiliation?.Serialize()
                ),
                this
            );
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

        public record PassablePositionFilter(MapManager Map, EntityLayer? Layer, IEnumerable<Vector2Int>? Area)
        {
            public PassablePositionFilter On(EntityLayer layer)
            {
                return new(Map, layer, Area);
            }
            public PassablePositionFilter In(IEnumerable<Vector2Int> area)
            {
                return new(Map, Layer, area);
            }
            public HashSet<Vector2Int> Get()
            {
                var result = Map.TilemapViewer.GetAllPassablePositions();
                if (Layer.HasValue)
                    result.ExceptWith(Map.GetAllEntityPositionsAt(Layer.Value));
                if (Area != null)
                    result.IntersectWith(Area);
                return result;
            }
        }

        public PassablePositionFilter BlankPositions() => new(this, null, null);
        public HashSet<Vector2Int> GetAllBlankPositionsOn(EntityLayer layer) => BlankPositions().On(layer).Get();
        public HashSet<Vector2Int> GetAllPassablePositions() => GetAllBlankPositionsOn(EntityLayer.Middle);
        public HashSet<Vector2Int> GetPassablePositionsInArea(IEnumerable<Vector2Int> area) => BlankPositions().In(area).Get();
        public HashSet<Vector2Int> GetAllLightPassablePositions()
        {
            return TilemapViewer.GetAllPassablePositions();
        }

        public bool IsPassable(Vector2Int position)
        {
            return IsMapPassable(position) && !GetAllEntityPositionsAt(EntityLayer.Middle).Contains(position);
        }

        public bool IsMapPassable(Vector2Int position)
        {
            return TilemapViewer.IsPassable(position);
        }

        public bool IsPassableIgnoreWall(Vector2Int position)
        {
            return !AllCharacterPositions().Contains(position);
        }

        public bool IsReachable(Vector2Int from, Vector2Int to)
        {
            var route = new AStar(GetAllPassablePositions()).Calc(from, to);
            if (route.Any())
                return false;
            if (IsPassable(to))
                return route.Last() == to;
            else
                return (route.Last() - to).sqrMagnitude <= 2;
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

            Observable.Merge(
                ((IEntityGroupEvents)CharacterManager.CharacterEvents).OnPositionChanged,
                ((IEntityGroupEvents)ItemManager.ItemEntityEvents).OnPositionChanged,
                ((IEntityGroupEvents)EventEntityManager.EventEntityEvents).OnPositionChanged
            ).Subscribe(positionChanged =>
                positionChanged.Entity.SetVisiblity(Player.IsVisible(positionChanged.Message.Position))
            ).AddTo(_disposables);

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
            if (turn % 30 == 0)
            {
                var positions = GetAllPassablePositions().Except(Player.VisionRange.VisibleArea);
                if (positions.Any())
                    SpawnRandomEnemy(positions.GetAtRandom());
            }
        }

        public void RemoveWalls(IEnumerable<Vector2Int> positions)
        {
            _tilemap.RemoveWalls(positions);
        }

        public record EntityFilter(MapManager Map, IEnumerable<IEntity> Entities, HashSet<Vector2Int>? cachedPositions, EntityLayer? Layer, IEnumerable<Vector2Int>? Area)
        {
            public EntityFilter On(EntityLayer layer)
            {
                return new(Map, Entities, null, layer, Area);
            }
            public EntityFilter In(IEnumerable<Vector2Int> area)
            {
                return new(Map, Entities, null, Layer, area);
            }
            private IEnumerable<IEntity> Get()
            {
                var result = Entities;
                if (Layer.HasValue)
                    result = result.Where(entity => entity.Layer == Layer.Value);
                if (Area != null)
                    result = result.Where(entity => Area.Contains(entity.CurrentPosition));
                return result.ToHashSet();
            }
            public HashSet<IEntity> GetEntities()
            {
                return Get().ToHashSet();
            }
            public HashSet<Vector2Int> GetPositions()
            {
                if (cachedPositions != null)
                    return cachedPositions;
                return Get().Select(entity => entity.CurrentPosition).ToHashSet();
            }
        }

        public EntityFilter AllEntities() => new(this, Entities, null, null, null);
        public EntityFilter AllItem() => new(this, ItemManager.Items, ItemManager.GetAllItemPositions(), null, null);
        public EntityFilter AllCharacter() => new(this, CharacterManager.Characters, CharacterManager.GetAllCharacterPositions(), null, null);
        public EntityFilter AllEventEntity() => new(this, EventEntityManager.EventEntities, null, null, null);
        public HashSet<Vector2Int> AllItemPositions() =>  AllItem().GetPositions();
        public HashSet<Vector2Int> AllCharacterPositions() => AllCharacter().GetPositions();
        public IEnumerable<Vector2Int> GetAllEntityPositionsAt(EntityLayer layer) => AllEntities().On(layer).GetPositions();

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