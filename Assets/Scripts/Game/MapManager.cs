#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Characters;
using Domain.Service.Characters.Behavior;
using Domain.Service.Entities;
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Logs;
using Domain.Service.Map;
using Domain.Service.Rooms;
using ObservableCollections;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Utilities.Algorithms;
using Random = UnityEngine.Random;

namespace Game
{
    public class MapManager : IDisposable, ISerializable<MapMemento>, IMap
    {
        public string Name => _dungeonData.Name;
        public readonly int Level;
        private readonly CompositeDisposable _disposables = new();
        private readonly Tilemap _tilemap;
        private DungeonMapData _dungeonData;
        private List<IEventArea> _eventAreas = new();
        private MonsterHouse? _monsterHouse;
        private Shop? _shop;
        public IShop? Shop => _shop;
        public ReadOnlyReactiveProperty<bool>? IsStolen => _shop?.IsStolen;
        public RectInt? ShopRect => _shop?.Rect;
        private ReactiveProperty<bool> _stairsLocked = new(true);
        public ReadOnlyReactiveProperty<bool> MovementEntityLocked => _stairsLocked;
        public ObservableList<ICharacter> KeyCharacters = new();
        public bool IsEventExecuting { get; private set; }

        public MapManager(MapMemento map, DungeonMapData data, CharacterMemento? playerData, List<CharacterMemento>? partyMembers,
            Vector2Int? playerPosition, CharacterControlInputReceiver receiver, int level)
        {
            Level = level;

            _tilemap = new Tilemap(map.Tilemap);

            if (playerPosition == null)
            {
                playerPosition = map.RandomBlankPosition;
            }

            if (playerData == null)
            {
                playerData = CharacterFactory.BuildPlayer("Player", playerPosition.Value);
            }
            else
            {
                playerData.Entity.Position = playerPosition.Value;
            }

            CharacterManager = new CharacterManager(playerData, receiver, this);
            ItemManager = new ItemManager();
            EventEntityManager = new EventEntityManager(map.EventEntities, _stairsLocked);
            ThrowAnimationEntityManager = new ThrowAnimationEntityManager();

            _dungeonData = data;

            if (partyMembers != null)
            {
                foreach (var character in partyMembers)
                {
                    character.Entity.Position = FindBlankPositionFrom(playerPosition.Value,
                                position => !AllCharacterPositions().Contains(position));
                    CharacterManager.SpawnCharacter(character, this);
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

            if (map.MonsterHouse.HasValue)
            {
                _monsterHouse = new MonsterHouse(map.MonsterHouse.Value, Player.CurrentPosition);
                _eventAreas.Add(_monsterHouse);
            }

            if (map.Shop.HasValue)
            {
                var clerk = Characters.FirstOrDefault(character => character.Id == new Id<IEntity>(map.Shop.Value.Clerk.Id));
                if (clerk == null && !map.Shop.Value.IsStolen)
                    clerk = CharacterManager.SpawnCharacter(CharacterFactory.BuildCharacter(_dungeonData.Clerk, BlankPositions().In(map.Shop.Value.Room.Room.RectRange()).Get().GetAtRandom(), isSlept: false, isShiny: false, hasHomePosition: true), this);
                if (clerk != null)
                {
                    _shop = new Shop(map.Shop.Value, clerk, this);
                    EventEntityManager.Add(_shop.Clerk);
                    _eventAreas.Add(_shop);
                }
            }

            KeyCharacters = new(map.KeyCharacters
                .Select(character => GetCharacterFromId(new Id<IEntity>(character)))
                .Where(character => character != null)
                .Cast<ICharacter>()
            );
            if (KeyCharacters.Any())
            {
                KeyCharacters.ForEach(character => _disposables.Add(character.OnDead.Subscribe(_ => KeyCharacters.Remove(character))));
                _disposables.Add(KeyCharacters.ObserveCountChanged().Subscribe(count => Debug.Log(count)));
                _disposables.Add(KeyCharacters.ObserveCountChanged().Where(count => count == 0).Subscribe(_ => _stairsLocked.Value = false));
            }
            else
            {
                _stairsLocked.Value = false;
            }

            var visibleArea = CharacterManager.Player.VisionRange.VisibleArea;
            _tilemap.SetTilesKnown(visibleArea, true);

            foreach (var entity in Entities)
                entity.SetVisibility(visibleArea.Contains(entity.CurrentPosition));

            if (map.MonsterHouse.HasValue || map.Characters.Any(character => character.IsShiny))
            {
                GameLog.Add("<color=yellow>不穏な気配を感じる……</color>");
            }
        }

        public ICharacter? Player => CharacterManager?.Player;
        public CharacterManager CharacterManager { get; init; }
        public IObservableCollection<IEventEntity> EventEntities => EventEntityManager.EventEntities;
        public IObservableCollection<IIconEventEntity> EventEntitiesAndIcons => EventEntityManager.EventEntitiesAndIcons;
        public IObservableCollection<ThrowAnimationEntity> ThrowAnimationEntities => ThrowAnimationEntityManager.ThrowAnimationEntities;
        public ItemManager ItemManager { get; init; }
        public EventEntityManager EventEntityManager { get; init; }
        public ThrowAnimationEntityManager ThrowAnimationEntityManager { get; init; }

        public void Dispose()
        {
            CharacterManager.Dispose();
            ItemManager.Dispose();
            EventEntities.ForEach(eventEntity => eventEntity.Dispose());
            ThrowAnimationEntities.ForEach(throwAnimationEntity => throwAnimationEntity.Dispose());
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

                foreach (var throwAnimationEntity in ThrowAnimationEntities)
                    yield return throwAnimationEntity;
            }
        }

        public IItemEntity SpawnItem(IItem item, Vector2Int position) => ItemManager.SpawnItem(item, FindBlankPositionFrom(position, position => IsBlank(position, EntityLayer.Bottom)));
        public ICharacter SpawnEnemy(EnemyData enemy, Vector2Int position, IAffiliation? affiliation = null, bool? isSlept = null, bool? isShiny = null)
        {
            return CharacterManager.SpawnCharacter(
                CharacterFactory.BuildCharacter(
                    enemy,
                    FindBlankPositionFrom(position, position => IsBlank(position, EntityLayer.Middle)),
                    isSlept ?? Random.value < _dungeonData.SleepChance,
                    isShiny ?? Random.value < _dungeonData.ShinyChance,
                    affiliation?.Serialize()
                ),
                this
            );
        }
        public ICharacter SpawnRandomEnemy(Vector2Int position, bool? isShiny = null) => SpawnEnemy(_dungeonData.Enemies.GetRandomItem(), position, isShiny: isShiny);
        public async UniTask<Vector2Int> ShowThrowAnimation(Sprite icon, Vector2Int position, Direction8 direction, params EntityLayer[] canHitLayer)
        {
            var throwAnimationEntity = new ThrowAnimationEntity(position, icon);
            ThrowAnimationEntityManager.Add(throwAnimationEntity);
            var destination = await throwAnimationEntity.Throw(direction, this, canHitLayer);
            throwAnimationEntity.Destroy();
            return destination;
        }
        public ICharacter? GetCharacterFromId(Id<IEntity> id)
        {
            var character = CharacterManager.Characters.FirstOrDefault(character => character.Id == id);
            return character;
        }
        public IItem? GetItemFromId(Id<IItem> id)
        {
            var itemEntity = ItemManager.Items.FirstOrDefault(item => item.Item.Id == id);
            var item = itemEntity?.Item ?? Player.Inventory.AllItems.FirstOrDefault(i => i.Id == id);
            return item;
        }

        public IEnumerable<Vector2Int> GetVisibleAllyPositions(IHasAffiliation character, IEnumerable<Vector2Int> visibleArea)
        {
            return GetCharacterPositions(character, visibleArea, CharacterRelation.Ally);
        }

        public IEnumerable<Vector2Int> GetVisibleNeutralPositions(IHasAffiliation character, IEnumerable<Vector2Int> visibleArea)
        {
            return GetCharacterPositions(character, visibleArea, CharacterRelation.Neutral);
        }

        public IEnumerable<Vector2Int> GetVisibleEnemyPositions(IHasAffiliation character, IEnumerable<Vector2Int> visibleArea)
        {
            return GetCharacterPositions(character, visibleArea, CharacterRelation.Enemy);
        }

        public IEnumerable<Vector2Int> GetCharacterPositions(IHasAffiliation character, IEnumerable<Vector2Int> visibleArea, CharacterRelation relation)
        {
            return Characters.Where(c => relation.MatchesRelation(c, character)).Select(c => c.CurrentPosition).Where(p => visibleArea.Contains(p));
        }

        public IEventEntity? GetEventEntityAt(Vector2Int position, EntityLayer layer)
        {
            return EventEntities
                .Where(eventEntity => eventEntity.CurrentPosition == position)
                .Where(eventEntity => eventEntity.Layer == layer)
                .Where(eventEntity => eventEntity.Events.Any(e => e.CanExecuteEvent()))
                .FirstOrDefault();
        }

        public record PassablePositionFilter(MapManager Map, EntityLayer[] Layers, IEnumerable<Vector2Int>? Area)
        {
            public PassablePositionFilter On(params EntityLayer[] layers)
            {
                return new(Map, layers, Area);
            }
            public PassablePositionFilter In(IEnumerable<Vector2Int> area)
            {
                return new(Map, Layers, area);
            }
            public HashSet<Vector2Int> Get()
            {
                var result = Map.TilemapViewer.GetAllPassablePositions();
                if (Layers.Any())
                    foreach (var layer in Layers)
                        result.ExceptWith(Map.GetAllEntityPositionsAt(layer));
                if (Area != null)
                    result.IntersectWith(Area);
                return result;
            }
        }

        public PassablePositionFilter BlankPositions() => new(this, Array.Empty<EntityLayer>(), null);
        public HashSet<Vector2Int> GetAllBlankPositionsOn(EntityLayer layer) => BlankPositions().On(layer).Get();
        public HashSet<Vector2Int> GetAllPassablePositions() => GetAllBlankPositionsOn(EntityLayer.Middle);
        public HashSet<Vector2Int> GetPassablePositionsInArea(IEnumerable<Vector2Int> area) => BlankPositions().In(area).Get();
        public HashSet<Vector2Int> GetAllLightPassablePositions()
        {
            return TilemapViewer.GetAllPassablePositions();
        }

        public bool IsOverlapped(Vector2Int position, EntityLayer layer) => AllEntities().On(layer).Get().Count(entity => entity.CurrentPosition == position) > 1;
        public bool IsBlank(Vector2Int position, params EntityLayer[] layers) => BlankPositions().On(layers).Get().Contains(position);

        public bool IsPassable(Vector2Int position)
        {
            return IsPassableOnMap(position) && !AllEntities().On(EntityLayer.Middle).GetPositions().Contains(position);
        }

        public bool IsPassableOnMap(Vector2Int position)
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

        public void RemoveEventEntity(IEventEntity eventEntity)
        {
            EventEntityManager.Remove(eventEntity);
        }

        public ITilemapViewer TilemapViewer => _tilemap;

        public MapMemento Serialize()
        {
            var characters = Characters.ToList();
            characters.Remove(Player);
            return new MapMemento
            {
                Tilemap = _tilemap.Serialize(),
                Characters = characters.Select(character => character.Serialize()).ToList(),
                Items = ItemManager.Items.Select(item => item.Serialize()).ToList(),
                EventEntities = EventEntityManager.Serialize(),
                KeyCharacters = KeyCharacters.Select(character => character.Id.ToString()).ToList(),
                MonsterHouse = new(_monsterHouse?.Serialize()),
                Shop = new(_shop?.Serialize()),
                RandomBlankPosition = GetAllBlankPositionsOn(EntityLayer.Bottom).GetAtRandom()
            };
        }

        public MapMemento SerializeWithoutPartyMembers()
        {
            var characters = Characters.ToList();
            characters.Remove(Player);
            characters.RemoveAll(character => GetFollowingCharacters().Contains(character));
            return new MapMemento
            {
                Tilemap = _tilemap.Serialize(),
                Characters = characters.Select(character => character.Serialize()).ToList(),
                Items = ItemManager.Items.Select(item => item.Serialize()).ToList(),
                EventEntities = EventEntityManager.Serialize(),
                KeyCharacters = KeyCharacters.Select(character => character.Id.ToString()).ToList(),
                MonsterHouse = new(_monsterHouse?.Serialize()),
                Shop = new(_shop?.Serialize())
            };
        }

        private void SetRules()
        {
            CharacterManager.CharacterEvents.OnDead.Subscribe(dead =>
            {
                DropAllItem(dead.Character);
            }).AddTo(_disposables);

            CharacterManager.PlayerEvents.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                _tilemap.SetTilesKnown(areaChanged.Message.NewArea, true);

                foreach (var entity in Entities)
                    entity.SetVisibility(areaChanged.Message.NewArea.Contains(entity.CurrentPosition));
            }).AddTo(_disposables);

            CharacterManager.PlayerEvents.OnPositionChanged.Subscribe(async positionChanged =>
            {
                IsEventExecuting = true;
                var eventEntity = GetEventEntityAt(positionChanged.Message.Position, EntityLayer.Bottom);
                if (eventEntity != null)
                {
                    var choices = new List<string>();
                    foreach (var eventData in eventEntity.Events)
                    {
                        choices.Add(eventData.ChoiceText);
                    }
                    if (eventEntity.CanBeCanceled)
                    {
                        choices.Add("やめる");
                    }
                    var choiceIndex = 0;
                    if (choices.Count > 1)
                    {
                        choiceIndex = await Globals.GameManager.GetChoice(eventEntity.ChoiceMessage, choices.ToArray());
                    }
                    switch (choices[choiceIndex])
                    {
                        case "やめる":
                            break;
                        default:
                            await eventEntity.Events[choiceIndex].DoEvent(Globals.GameManager, this);
                            break;
                    }
                }

                foreach (var eventArea in _eventAreas)
                {
                    await eventArea.UpdatePosition(Globals.GameManager, this, positionChanged.Message.Position);
                }
                IsEventExecuting = false;
            }).AddTo(_disposables);

            CharacterManager.CharacterEvents.OnPositionChanged.Subscribe(positionChanged =>
            {
                if (positionChanged.Character.CanPickUp)
                {
                    if (positionChanged.Character.Inventory.HasEmptySpace())
                    {
                        var item = ItemManager.TryPickUp(positionChanged.Message.Position, positionChanged.Character == Player && Settings.AutoPickUpShopItem.Value);
                        if (item != null)
                        {
                            if (positionChanged.Character.TryPickUp(item.Item))
                            {
                                if (positionChanged.Character == Player)
                                    GameLog.Add($"{Player.GetName(Player)}は<color=yellow>{item.Item.Name}</color>を拾った");
                            }
                            else
                            {
                                Log.Error("cannot pick up item");
                            }
                        }
                    }
                }
            }).AddTo(_disposables);

            Observable.Merge(
                ((IEntityGroupEvents)CharacterManager.CharacterEvents).OnPositionChanged,
                ((IEntityGroupEvents)ItemManager.ItemEntityEvents).OnPositionChanged,
                ((IEntityGroupEvents)EventEntityManager.EventEntityEvents).OnPositionChanged,
                ((IEntityGroupEvents)ThrowAnimationEntityManager.EntityEvents).OnPositionChanged
            ).Subscribe(positionChanged =>
                positionChanged.Entity.SetVisibility(Player.IsVisible(positionChanged.Message.Position))
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
            if (turn % 100 == 0)
            {
                var positions = GetAllPassablePositions().Except(Player.VisionRange.VisibleArea);
                if (positions.Any())
                    SpawnRandomEnemy(positions.GetAtRandom(), false);
            }
        }

        public void RemoveWalls(IEnumerable<Vector2Int> positions)
        {
            _tilemap.RemoveWalls(positions);
        }

        public record EntityFilter<T>(MapManager Map, IEnumerable<T> Entities, EntityLayer[] Layers, IEnumerable<Vector2Int>? Area) where T : IEntity
        {
            public EntityFilter<T> On(params EntityLayer[] layers)
            {
                return new(Map, Entities, layers, Area);
            }
            public EntityFilter<T> In(IEnumerable<Vector2Int> area)
            {
                return new(Map, Entities, Layers, area);
            }
            public IEnumerable<T> Get()
            {
                var result = Entities;
                if (Layers.Any())
                    result = result.Where(entity => Layers.Contains(entity.Layer));
                if (Area != null)
                    result = result.Where(entity => Area.Contains(entity.CurrentPosition));
                return result.ToHashSet();
            }
            public HashSet<T> GetEntities()
            {
                return Get().ToHashSet();
            }
            public HashSet<Vector2Int> GetPositions()
            {
                return Get().Select(entity => entity.CurrentPosition).ToHashSet();
            }
        }

        public EntityFilter<IEntity> AllEntities() => new(this, Entities, Array.Empty<EntityLayer>(), null);
        public EntityFilter<IItemEntity> AllItem() => new(this, ItemManager.Items, Array.Empty<EntityLayer>(), null);
        public EntityFilter<ICharacter> AllCharacter() => new(this, CharacterManager.Characters, Array.Empty<EntityLayer>(), null);
        public EntityFilter<IEventEntity> AllEventEntity() => new(this, EventEntityManager.EventEntities, Array.Empty<EntityLayer>(), null);
        public HashSet<Vector2Int> AllItemPositions() => ItemManager.GetAllItemPositions();
        public HashSet<Vector2Int> AllCharacterPositions() => CharacterManager.GetAllCharacterPositions();
        public HashSet<Vector2Int> GetAllEntityPositionsAt(EntityLayer layer) => AllEntities().On(layer).GetPositions();
        public HashSet<IEntity> GetEntitiesInArea(IEnumerable<Vector2Int> area) => AllEntities().In(area).GetEntities();
        public HashSet<ICharacter> GetCharactersInArea(IEnumerable<Vector2Int> area) => AllCharacter().In(area).GetEntities();
        public HashSet<IItemEntity> GetItemsInArea(IEnumerable<Vector2Int> area) => AllItem().In(area).GetEntities();

        public void HandleItemDrop(int inventoryIndex)
        {
            var itemEntity = ItemManager.TryPickUp(Player.CurrentPosition, true);
            if (itemEntity != null)
            {
                GameLog.Add($"{Player.GetName(Player)}は{itemEntity.Item.Name}を拾った");
            }
            var item = Player.ReplaceInventory(itemEntity?.Item, inventoryIndex);
            if (item != null)
            {
                GameLog.Add($"{Player.GetName(Player)}は{item.Name}を捨てた.");
                ItemManager.SpawnItem(item, FindBlankPositionFrom(Player.CurrentPosition, position => IsBlank(position, EntityLayer.Bottom)));
            }
        }

        public void DropAllItem(ICharacter character)
        {
            for (var index = 0; index < character.Inventory.MaxItemCount; index++)
            {
                var item = character.ReplaceInventory(null, index);
                if (item != null)
                    ItemManager.SpawnItem(item, FindBlankPositionFrom(character.CurrentPosition, position => IsBlank(position, EntityLayer.Bottom)));
            }
        }

        /// <summary>
        ///     Gets a character that follows the player when moving from one map to another.
        ///     Does not include the player themselves.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ICharacter> GetFollowingCharacters()
        {
            return CharacterManager.Characters
                .Where(character => character != Player)
                .Where(character => !character.HasHomePosition)
                .Where(character => character.IsAlly(Player))
                .Where(character => character.IsVisible(Player.CurrentPosition));
        }

        public Vector2Int FindBlankPositionFrom(Vector2Int position, Func<Vector2Int, bool> isBlankFunc)
        {
            return BlankFinder.FindBlankPosition(isBlankFunc, TilemapViewer.IsPassable, position);
        }
    }
}