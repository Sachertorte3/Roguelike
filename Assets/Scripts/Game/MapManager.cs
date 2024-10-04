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
using Domain.Service.Items;
using Domain.Service.Logs;
using Domain.Service.Map;
using Domain.Service.Rooms;
using ObservableCollections;
using R3;
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
                playerData = playerData.ReplacePosition(playerPosition.Value);
            }

            CharacterManager = new CharacterManager(playerData, receiver, this);
            ItemManager = new ItemManager();
            EventEntityManager = new EventEntityManager(map.EventEntities, _stairsLocked);
            ThrowAnimationEntityManager = new ThrowAnimationEntityManager();

            _dungeonData = data;

            foreach (var character in map.Characters)
            {
                var ally = CharacterManager.SpawnAlly(character, this);
                EventEntityManager.Add(ally);
            }

            if (partyMembers != null)
            {
                foreach (var character in partyMembers)
                {
                    var ally = CharacterManager.SpawnAlly(
                        character.ReplacePosition(
                            FindBlankPositionFrom(
                                playerPosition.Value,
                                position => !AllCharacterPositions().Contains(position)
                            )
                        ),
                        this
                    );
                    EventEntityManager.Add(ally);
                }
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
                {
                    var clerkPosition = BlankPositions().In(map.Shop.Value.Room.Room.RectRange()).Get().GetAtRandom();
                    var ally = CharacterManager.SpawnAlly(CharacterFactory.BuildCharacter(_dungeonData.Clerk, clerkPosition, homePosition: clerkPosition), this);
                    EventEntityManager.Add(ally);
                    clerk = ally.Character;
                }
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

            if ((map.MonsterHouse.HasValue && !map.MonsterHouse.Value.HasEverEntered)
            || map.Characters.Any(character => character.IsShiny))
            {
                GameLog.Add("<color=yellow>不穏な気配を感じる……</color>");
            }
        }

        public ICharacter? Player => CharacterManager?.Player;
        public CharacterManager CharacterManager { get; init; }
        public IObservableCollection<IEventEntity> EventEntities => EventEntityManager.EventEntities;
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
        public class EntityIdComparer : IEqualityComparer<IEntity>
        {
            public bool Equals(IEntity? x, IEntity? y)
            {
                return x?.Id == y?.Id;
            }

            public int GetHashCode(IEntity obj)
            {
                return obj.Id.GetHashCode();
            }
        }
        public IEnumerable<IEntity> Entities => Characters
                    .Cast<IEntity>()
                    .Concat(Items)
                    .Concat(EventEntities)
                    .Concat(ThrowAnimationEntities)
                    .Distinct(new EntityIdComparer());

        public IItemEntity SpawnItem(IItem item, Vector2Int position) => ItemManager.SpawnItem(item, FindBlankPositionFrom(position, position => IsBlankAndStandable(position, EntityLayer.Bottom)));
        public ICharacter SpawnEnemy(EnemyData enemy, Vector2Int position, IAffiliation? affiliation = null, bool? isSlept = null, bool? isShiny = null)
        {
            var ally = CharacterManager.SpawnAlly(
                CharacterFactory.BuildCharacter(
                    enemy,
                    FindBlankPositionFrom(position, position => IsBlankAndStandable(position, EntityLayer.Middle)),
                    isSlept: isSlept ?? Random.value < _dungeonData.SleepChance,
                    isShiny: isShiny ?? Random.value < _dungeonData.ShinyChance,
                    affiliation: affiliation?.Serialize()
                ),
                this
            );
            EventEntityManager.Add(ally);
            return ally.Character;
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
                if (layers.Any())
                    return new(Map, layers, Area);
                else
                    return this;
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
                else
                    result.ExceptWith(Map.AllEntities().GetPositions());
                if (Area != null)
                    result.IntersectWith(Area);
                return result;
            }
        }

        public PassablePositionFilter BlankPositions() => new(this, Array.Empty<EntityLayer>(), null);
        public HashSet<Vector2Int> GetAllBlankPositionsOn(params EntityLayer[] layers) => BlankPositions().On(layers).Get();
        public HashSet<Vector2Int> GetAllBlankAndStandablePositionsOn(params EntityLayer[] layers) => GetAllBlankPositionsOn(layers);
        public HashSet<Vector2Int> GetAllPassablePositions(IAffiliation affiliation)
        {
            var passablePositions = GetAllBlankPositionsOn(EntityLayer.Middle);

            var swapableCharacters = AllEntities().On(EntityLayer.Middle).Get()
            .Where(entity => entity is ICharacter character && !character.Affiliation.IsEnemy(affiliation));

            foreach (var character in swapableCharacters)
                passablePositions.Add(character.CurrentPosition);
            return passablePositions;
        }
        public HashSet<Vector2Int> GetPassablePositionsInArea(IEnumerable<Vector2Int> area)
        {
            return GetAllPassablePositions(Player.Affiliation).Where(position => area.Contains(position)).ToHashSet();
        }
        public HashSet<Vector2Int> GetAllLightPassablePositions()
        {
            return TilemapViewer.GetAllLightPassablePositions();
        }

        public bool IsOverlapped(Vector2Int position, EntityLayer layer) => AllEntities().On(layer).Get().Count(entity => entity.CurrentPosition == position) > 1;
        public bool IsBlank(Vector2Int position, params EntityLayer[] layers) => BlankPositions().On(layers).Get().Contains(position);
        public bool IsBlankAndStandable(Vector2Int position, params EntityLayer[] layers)
        {
            if (!IsWalkableOnMap(position))
                return false;
            var entity = AllEntities().On(EntityLayer.Middle).At(position).Get().FirstOrDefault();
            if (entity == null)
                return true;
            return false;
        }
        public bool IsWalkable(Vector2Int position, IAffiliation actor)
        {
            if (!IsWalkableOnMap(position))
                return false;
            var entity = AllEntities().On(EntityLayer.Middle).At(position).Get().FirstOrDefault();
            if (entity == null)
                return true;
            if (entity is ICharacter character && character != Player)
                return !character.Affiliation.IsEnemy(actor);
            return false;
        }

        public bool IsWalkableOnMap(Vector2Int position)
        {
            return TilemapViewer.IsWalkable(position);
        }

        public bool IsPassableOnMap(Vector2Int position)
        {
            return TilemapViewer.IsPassable(position);
        }

        public bool IsPassableIgnoreWall(Vector2Int position)
        {
            return !AllCharacterPositions().Contains(position);
        }

        public bool IsReachable(Vector2Int from, Vector2Int to, IAffiliation actor)
        {
            var route = new AStar(GetAllPassablePositions(actor)).Calc(from, to);
            if (!route.Any())
                return false;
            if (IsWalkable(to, actor))
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
            (
                tilemap: _tilemap.Serialize(),
                characters: characters.Select(character => character.Serialize()).ToList(),
                items: ItemManager.Items.Select(item => item.Serialize()).ToList(),
                eventEntities: EventEntityManager.Serialize(),
                keyCharacters: KeyCharacters.Select(character => character.Id.ToString()).ToList(),
                monsterHouse: _monsterHouse.ToOption().Map(x => x.Serialize()),
                shop: _shop.ToOption().Map(x => x.Serialize()),
                randomBlankPosition: GetAllBlankPositionsOn(EntityLayer.Bottom).GetAtRandom()
            );
        }

        public MapMemento SerializeWithoutPartyMembers()
        {
            var characters = Characters.ToList();
            characters.Remove(Player);
            characters.RemoveAll(character => GetFollowingCharacters().Contains(character));
            return new MapMemento
            (
                tilemap: _tilemap.Serialize(),
                characters: characters.Select(character => character.Serialize()).ToList(),
                items: ItemManager.Items.Select(item => item.Serialize()).ToList(),
                eventEntities: EventEntityManager.Serialize(),
                keyCharacters: KeyCharacters.Select(character => character.Id.ToString()).ToList(),
                monsterHouse: _monsterHouse.ToOption().Map(x => x.Serialize()),
                shop: _shop.ToOption().Map(x => x.Serialize()),
                randomBlankPosition: GetAllBlankPositionsOn(EntityLayer.Bottom).GetAtRandom()
            );
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
                foreach (var eventArea in _eventAreas)
                {
                    await eventArea.UpdatePosition(Globals.GameManager, this, positionChanged.Message.Position);
                }

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
                IsEventExecuting = false;
            }).AddTo(_disposables);

            CharacterManager.CharacterEvents.OnPositionChanged.Subscribe(positionChanged =>
            {
                var item = ItemManager.GetItemAt(positionChanged.Message.Position);
                if (item != null)
                {
                    if (positionChanged.Character.CanPickUp
                        && positionChanged.Character.CanPickUpItem()
                        && ItemManager.CanPickUpAt(positionChanged.Message.Position, positionChanged.Character == Player && Settings.AutoPickUpShopItem.Value))
                    {
                        ItemManager.PickUpAt(positionChanged.Message.Position, positionChanged.Character == Player && Settings.AutoPickUpShopItem.Value);
                        if (positionChanged.Character.TryPickUp(item.Item))
                        {
                            if (positionChanged.Character == Player)
                                GameLog.Add($"{Player.GetName(Player)}は<color=yellow>{item.Item.Name}</color>を拾った");
                        }
                        else
                        {
                            throw new Exception("Unexpected error. Unable to pick up item.");
                        }
                    }
                    else
                    {
                        GameLog.Add($"{positionChanged.Character.GetName(positionChanged.Character)}は{item.Item.Name}の上に乗った");
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
            if (Random.value < 1 / 64f)
            {
                var positions = GetAllBlankPositionsOn(EntityLayer.Middle).Except(Player.VisionRange.VisibleArea);
                if (positions.Any())
                    SpawnRandomEnemy(positions.GetAtRandom(), isShiny: false);
            }
            _tilemap.UpdateTurn();
        }

        public void RemoveWalls(IEnumerable<Vector2Int> positions)
        {
            _tilemap.RemoveWalls(positions);
        }

        public void SetGrasses(IEnumerable<Vector2Int> positions, bool isGrass)
        {
            _tilemap.SetGrasses(positions, isGrass);
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
            public EntityFilter<T> At(Vector2Int position)
            {
                return new(Map, Entities, Layers, new[] { position });
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
            var itemEntity = ItemManager.TryPickUpAt(Player.CurrentPosition, true);
            if (itemEntity != null)
            {
                GameLog.Add($"{Player.GetName(Player)}は{itemEntity.Item.Name}を拾った");
            }
            var item = Player.ReplaceInventory(itemEntity?.Item, inventoryIndex);
            if (item != null)
            {
                GameLog.Add($"{Player.GetName(Player)}は{item.Name}を捨てた.");
                ItemManager.SpawnItem(item, FindBlankPositionFrom(Player.CurrentPosition, position => IsBlankAndStandable(position, EntityLayer.Bottom)));
            }
        }

        public void DropAllItem(ICharacter character)
        {
            for (var index = 0; index < character.Inventory.MaxItemCount; index++)
            {
                var item = character.ReplaceInventory(null, index);
                if (item != null)
                    ItemManager.SpawnItem(item, FindBlankPositionFrom(character.CurrentPosition, position => IsBlankAndStandable(position, EntityLayer.Bottom)));
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
            return BlankFinder.FindBlankPosition(isBlankFunc, TilemapViewer.IsWalkable, position);
        }
    }
}