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
using Domain.Model.Message;
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
using UnityEngine;
using Utilities;
using Utilities.Algorithms;
using Random = UnityEngine.Random;

namespace Game
{
    public class MapManager : IDisposable, ISerializable<MapMemento>, IMap
    {
        public Id<IMap> Id { get; init; }
        public Location Location { get; init; }
        public string Name => Location.MapName;
        public int Level => Location.Level;
        public SectionType Type => _dungeonData.Type;
        public ItemDatabase ItemDatabase => _dungeonData.ItemDatabase;
        public ItemPlaceholders ItemPlaceholders { get; init; }
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
        private int EventExecutionCount;
        public bool IsEventExecuting => EventExecutionCount > 0;
        private readonly Subject<OnEffectSpawnedMessage> _onEffectSpawned = new();
        public MapManager(MapMemento map, DungeonMapData data, CharacterMemento? playerData,
            List<CharacterMemento>? partyMembers,
            Vector2Int? playerPosition, CharacterControlInputReceiver receiver, ItemPlaceholders itemPlaceholders)
        {
            Id = map.Id;
            Location = map.Location;
            ItemPlaceholders = itemPlaceholders;

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
            FireEntityManager = new FireEntityManager(map.Fires);

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
                var clerk = Characters.FirstOrDefault(character =>
                    character.Id == map.Shop.Value.ClerkId);
                if (clerk == null && !map.Shop.Value.IsStolen)
                {
                    var clerkPosition = GetAllBlankPositionsOn(EntityLayer.Middle)
                        .In(map.Shop.Value.Room.Room.RectRange())
                        .GetAtRandom();
                    var ally = CharacterManager.SpawnAlly(
                        CharacterFactory.BuildCharacter(_dungeonData.Clerk, clerkPosition.Position, homePosition: (Location, clerkPosition.Position)),
                        this);
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

            KeyCharacters = new ObservableList<ICharacter>(map.KeyCharacters
                .Select(character => GetCharacterFromId(new Id<IEntity>(character)))
                .Where(character => character != null)
                .Cast<ICharacter>()
            );
            if (KeyCharacters.Any())
            {
                KeyCharacters.ForEach(character =>
                    _disposables.Add(character.OnDead.Subscribe(_ => KeyCharacters.Remove(character))));
                _disposables.Add(KeyCharacters.ObserveCountChanged().Subscribe(count => Debug.Log(count)));
                _disposables.Add(KeyCharacters.ObserveCountChanged().Where(count => count == 0)
                    .Subscribe(_ => _stairsLocked.Value = false));
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

        public IObservableCollection<ThrowAnimationEntity> ThrowAnimationEntities =>
            ThrowAnimationEntityManager.ThrowAnimationEntities;

        public IObservableCollection<Fire> FireEntities => FireEntityManager.FireEntities;

        public ItemManager ItemManager { get; init; }
        public EventEntityManager EventEntityManager { get; init; }
        public ThrowAnimationEntityManager ThrowAnimationEntityManager { get; init; }
        public FireEntityManager FireEntityManager { get; init; }
        public Observable<OnEffectSpawnedMessage> OnEffectSpawned => _onEffectSpawned;

        public void Dispose()
        {
            CharacterManager.Dispose();
            ItemManager.Dispose();
            EventEntities.ForEach(eventEntity => eventEntity.Dispose());
            ThrowAnimationEntities.ForEach(throwAnimationEntity => throwAnimationEntity.Dispose());
            FireEntities.ForEach(fireEntity => fireEntity.Dispose());
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
            .Concat(FireEntities)
            .Distinct(new EntityIdComparer());

        public IItemEntity SpawnItem(IItem item, Vector2Int position)
        {
            return ItemManager.SpawnItem(item,
                FindBlankPositionFrom(position, position => At(position).IsBlankAndStandable(EntityLayer.Bottom)));
        }

        public ICharacter SpawnEnemy(EnemyData enemy, Vector2Int position, IAffiliation? affiliation = null,
            bool? isSlept = null, bool? isShiny = null)
        {
            var ally = CharacterManager.SpawnAlly(
                CharacterFactory.BuildCharacter(
                    enemy,
                    FindBlankPositionFrom(position, position => At(position).IsBlankAndStandable(EntityLayer.Middle)),
                    isSlept: isSlept ?? Random.value < _dungeonData.SleepChance,
                    isShiny: isShiny ?? Random.value < _dungeonData.ShinyChance,
                    affiliation: affiliation
                ),
                this
            );
            EventEntityManager.Add(ally);
            return ally.Character;
        }

        public ICharacter SpawnRandomEnemy(Vector2Int position, bool? isSlept = null, bool? isShiny = null)
        {
            return SpawnEnemy(_dungeonData.Enemies.GetRandomItem(), position, isSlept: isSlept, isShiny: isShiny);
        }

        public async UniTask<Vector2Int> ShowThrowAnimation(Sprite icon, Vector2Int position, Direction8 direction,
            int distance, params EntityLayer[] canHitLayer)
        {
            var throwAnimationEntity = new ThrowAnimationEntity(position, icon);
            ThrowAnimationEntityManager.Add(throwAnimationEntity);
            var destination = await throwAnimationEntity.Throw(direction, this, distance, canHitLayer);
            throwAnimationEntity.Destroy();
            return destination;
        }

        public void SpawnEffect(IEnumerable<Vector2Int> area, Color color)
        {
            _onEffectSpawned.OnNext(new OnEffectSpawnedMessage(area, color));
        }

        public IMapPosition At(Vector2Int position)
        {
            return new MapPosition(position, this, TilemapViewer);
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

        public List<IEventEntity> GetEventEntityAt(Vector2Int position, EntityLayer layer)
        {
            return EventEntities
                .Where(eventEntity => eventEntity.CurrentPosition == position)
                .Where(eventEntity => eventEntity.Layer == layer)
                .ToList();
        }

        public bool IsGrass(Vector2Int position)
        {
            return TilemapViewer.IsGrass(position);
        }

        public bool IsFireAt(Vector2Int position)
        {
            return FireEntities.Any(fire => fire.CurrentPosition == position);
        }

        public async UniTask ExecuteTrapAt(Vector2Int position, ICharacter actor)
        {
            var eventEntities = GetEventEntityAt(position, EntityLayer.Bottom);
            foreach (var eventEntity in eventEntities)
            {
                if (eventEntity is Trap trapEntity)
                {
                    await trapEntity.Event.DoEvent(actor, Globals.GameManager, this);
                }
            }
        }

        public bool IsInside(Vector2Int position)
        {
            return _tilemap.IsPositionInsideMap(position);
        }

        public HashSet<Vector2Int> GetAllPositions()
        {
            return _tilemap.GetAllTiles().Select(tile => tile.position).ToHashSet();
        }

        public IEnumerable<IMapPosition> GetAllBlankPositionsOn(params EntityLayer[] layers)
        {
            return TilemapViewer
                .GetAllPassablePositions()
                .Select(position => At(position))
                .Where(position => position.IsBlank(layers));
        }

        public IEnumerable<IMapPosition> GetAllBlankAndStandablePositionsOn(params EntityLayer[] layers)
        {
            return TilemapViewer
                .GetAllWalkablePositions()
                .Select(position => At(position))
                .Where(position => position.IsBlank(layers));
        }

        public IEnumerable<IMapPosition> GetAllWalkablePositions(IAffiliation affiliation)
        {
            var result = TilemapViewer.GetAllWalkablePositions();
            result.ExceptWith(
                Entities
                    .On(EntityLayer.Middle)
                    .Where(entity => !(entity is ICharacter character && !character.Affiliation.IsEnemy(affiliation)))
                    .Positions());
            return result.Select(position => At(position));
        }

        public IEnumerable<IMapPosition> GetBlankAndStandablePositionsInArea(IEnumerable<Vector2Int> area,
            params EntityLayer[] layers)
        {
            return GetAllBlankAndStandablePositionsOn(layers).In(area);
        }

        public HashSet<Vector2Int> GetAllLightPassablePositions()
        {
            return TilemapViewer.GetAllLightPassablePositions();
        }

        public bool IsReachable(Vector2Int from, Vector2Int to, IHasBehavior actor)
        {
            var calculator = new MoveCostCalculator(actor, this, true);
            var route = new AStar(calculator.Calculate).Calc(from, to);
            if (!route.Any())
                return false;
            if (At(to).IsWalkable(actor.Affiliation))
                return route.Last() == to;
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
                Id,
                Location,
                _tilemap.Serialize(),
                characters.Select(character => character.Serialize()).ToList(),
                ItemManager.Items.Select(item => item.Serialize()).ToList(),
                EventEntityManager.Serialize(),
                FireEntityManager.Serialize(),
                KeyCharacters.Select(character => character.Id.ToString()).ToList(),
                _monsterHouse.ToOption().Map(x => x.Serialize()),
                _shop.ToOption().Map(x => x.Serialize()),
                GetAllBlankPositionsOn(EntityLayer.Bottom, EntityLayer.Middle, EntityLayer.Top).GetAtRandom().Position
            );
        }

        public MapMemento SerializeWithoutPartyMembers()
        {
            var characters = Characters.ToList();
            characters.Remove(Player);
            characters.RemoveAll(character => GetFollowingCharacters().Contains(character));
            return new MapMemento
            (
                Id,
                Location,
                _tilemap.Serialize(),
                characters.Select(character => character.Serialize()).ToList(),
                ItemManager.Items.Select(item => item.Serialize()).ToList(),
                EventEntityManager.Serialize(),
                FireEntityManager.Serialize(),
                KeyCharacters.Select(character => character.Id.ToString()).ToList(),
                _monsterHouse.ToOption().Map(x => x.Serialize()),
                _shop.ToOption().Map(x => x.Serialize()),
                GetAllBlankPositionsOn(EntityLayer.Bottom, EntityLayer.Middle, EntityLayer.Top).GetAtRandom().Position
            );
        }

        private void SetRules()
        {
            CharacterManager.CharacterEvents.OnDead.Subscribe(dead => { DropAllItem(dead.Character); })
                .AddTo(_disposables);

            CharacterManager.PlayerEvents.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                _tilemap.SetTilesKnown(Player.VisionRange.VisibleArea, true);

                foreach (var entity in Entities)
                    entity.SetVisibility(Player.IsVisible(entity.CurrentPosition));
            }).AddTo(_disposables);

            CharacterManager.PlayerEvents.OnPositionChanged.Subscribe(async positionChanged =>
            {
                EventExecutionCount++;
                foreach (var eventArea in _eventAreas)
                {
                    await eventArea.UpdatePosition(Globals.GameManager, this, positionChanged.Message.Position);
                }
                EventExecutionCount--;
            }).AddTo(_disposables);

            CharacterManager.CharacterEvents.OnPositionChanged.Subscribe(async positionChanged =>
            {
                var item = ItemManager.GetItemAt(positionChanged.Message.Position);
                if (item != null)
                {
                    if (positionChanged.Character.CanPickUp
                        && positionChanged.Character.CanPickUpItem()
                        && ItemManager.CanPickUpAt(positionChanged.Message.Position,
                            positionChanged.Character == Player && Settings.AutoPickUpShopItem.Value))
                    {
                        ItemManager.PickUpAt(positionChanged.Message.Position,
                            positionChanged.Character == Player && Settings.AutoPickUpShopItem.Value);
                        if (positionChanged.Character.TryAddToInventory(item.Item))
                        {
                            if (positionChanged.Character == Player)
                                GameLog.Add($"{Player.GetName(Player)}は<color=yellow>{item.Item.GetName(Player, ItemPlaceholders)}</color>を拾った");
                        }
                        else
                        {
                            throw new Exception("Unexpected error. Unable to pick up item.");
                        }
                    }
                    else
                    {
                        GameLog.Add(
                            $"{positionChanged.Character.GetName(Player)}は{item.Item.GetName(Player, ItemPlaceholders)}の上に乗った");
                    }
                }

                EventExecutionCount++;
                var eventEntities = GetEventEntityAt(positionChanged.Message.Position, EntityLayer.Bottom);
                foreach (var eventEntity in eventEntities)
                {
                    if (positionChanged.Character == Player || !eventEntity.Event.IsPlayerOnly)
                        await eventEntity.Event.DoEvent(positionChanged.Character, Globals.GameManager, this);
                }
                EventExecutionCount--;
            }).AddTo(_disposables);

            CharacterManager.CharacterEvents.OnAffectedByTrapFlagsChanged.Subscribe(async affectedByTrap =>
            {
                EventExecutionCount++;
                if (affectedByTrap.Message.IsAffectedByTrap)
                {
                    await ExecuteTrapAt(affectedByTrap.Character.CurrentPosition, affectedByTrap.Character);
                }
                EventExecutionCount--;
            }).AddTo(_disposables);

            Observable.Merge(
                ((IEntityGroupEvents)CharacterManager.CharacterEvents).OnPositionChanged,
                ((IEntityGroupEvents)ItemManager.ItemEntityEvents).OnPositionChanged,
                ((IEntityGroupEvents)EventEntityManager.EventEntityEvents).OnPositionChanged,
                ((IEntityGroupEvents)ThrowAnimationEntityManager.EntityEvents).OnPositionChanged,
                ((IEntityGroupEvents)FireEntityManager.EntityEvents).OnPositionChanged
            ).Subscribe(positionChanged =>
                positionChanged.Entity.SetVisibility(Player.IsVisible(positionChanged.Message.Position))
            ).AddTo(_disposables);

            _tilemap.OnTilesChanged.Subscribe(tileChanged =>
            {
                _fullVisibleArea = null;
                foreach (var (position, _) in tileChanged)
                {
                    var visibleArea = GetVisibleArea(position);
                    foreach (var pos in visibleArea)
                    {
                        _visionCache.Remove(pos);
                    }
                    _tilemap.SetTilesKnown(visibleArea, true);
                    Characters.In(visibleArea).ForEach(character => character.VisionRange.Refresh());
                }
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
                var positions = GetAllBlankPositionsOn(EntityLayer.Middle).Values().Except(Player.VisionRange.VisibleArea);
                if (positions.Any())
                    SpawnRandomEnemy(positions.GetAtRandom(), null, false);
            }

            FireEntityManager.UpdateTurn(this);

            var characters = Characters.In(FireEntityManager.FireEntities.Positions());
            foreach (var character in characters)
            {
                character.LoseHp(1);
                GameLog.Add($"{character.GetName(Player)}は火に焼かれた");
            }
            var items = Items.In(FireEntityManager.FireEntities.Positions());
            foreach (var item in items)
            {
                item.Destroy();
                GameLog.Add($"{item.Item.GetName(Player, ItemPlaceholders)}は灰になった");
            }

            SetGrasses(FireEntityManager.FireEntities.Positions(), false);

            _tilemap.UpdateTurn();
        }

        public void RemoveWalls(IEnumerable<Vector2Int> positions)
        {
            _tilemap.RemoveWalls(positions);
        }

        public void SetGrasses(IEnumerable<Vector2Int> positions, bool isGrass)
        {
            _tilemap.SetOverlayTiles(positions, isGrass ? OverlayTileCategory.Grass : null);
        }

        public void SetIce(IEnumerable<Vector2Int> positions, bool isIce)
        {
            _tilemap.SetOverlayTiles(positions, isIce ? OverlayTileCategory.FloatingIce : null);
        }

        public void SpawnFire(IEnumerable<Vector2Int> positions)
        {
            foreach (var position in positions)
            {
                if (At(position).CanPlace(false, false, true))
                    FireEntityManager.Add(new Fire(Fire.Build(position)));
            }
        }

        public HashSet<Vector2Int> AllItemPositions()
        {
            return ItemManager.GetAllItemPositions();
        }

        public HashSet<Vector2Int> AllCharacterPositions()
        {
            return CharacterManager.GetAllCharacterPositions();
        }

        public IItemEntity? TryPickUpAt(Vector2Int position, bool isShopItem) => ItemManager.TryPickUpAt(position, isShopItem);

        public void DropAllItem(ICharacter character)
        {
            for (var index = 0; index < character.Inventory.MaxItemCount; index++)
            {
                var item = character.ReplaceInventory(null, index);
                if (item != null)
                    SpawnItem(item,
                        FindBlankPositionFrom(character.CurrentPosition,
                            position => At(position).IsBlankAndStandable(EntityLayer.Bottom)));
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
                .Where(character => character.IsAlly(Player))
                .Where(character => character.IsVisible(Player.CurrentPosition));
        }

        public Vector2Int FindBlankPositionFrom(Vector2Int position, Func<Vector2Int, bool> isBlankFunc)
        {
            return BlankFinder.FindBlankPosition(isBlankFunc, TilemapViewer.IsWalkable, position);
        }

        private Dictionary<Vector2Int, HashSet<Vector2Int>> _visionCache = new();
        private HashSet<Vector2Int>? _fullVisibleArea;

        public bool IsVisible(Vector2Int from, Vector2Int to, float radius)
        {
            if ((from - to).sqrMagnitude > radius * radius)
                return false;
            if (_visionCache.TryGetValue(from, out var area))
                return area.Contains(to);
            else if (_visionCache.TryGetValue(to, out area))
                return area.Contains(from);
            UpdateVisibleAreaCache(from);
            return _visionCache[from].Contains(to);
        }

        public HashSet<Vector2Int> GetVisibleArea(Vector2Int from, float radius)
        {
            return GetVisibleArea(from).Where(x => (x - from).sqrMagnitude <= radius * radius).ToHashSet();
        }

        public HashSet<Vector2Int> GetVisibleArea(Vector2Int from)
        {
            if (_visionCache.TryGetValue(from, out var area))
                return area;
            UpdateVisibleAreaCache(from);
            return _visionCache[from];
        }

        public HashSet<Vector2Int> GetFullVisibleArea()
        {
            if (_fullVisibleArea == null)
                _fullVisibleArea = ViewCalculator.ComputeFullVisibility(_tilemap.GetAllLightPassablePositions());
            return _fullVisibleArea;
        }

        private void UpdateVisibleAreaCache(Vector2Int from)
        {
            _visionCache[from] = ViewCalculator.FieldOfView(from, _tilemap.Size, pos => !At(pos).IsLightPassable());
        }

        public HashSet<Vector2Int> ComputeCircle(HashSet<Vector2Int> passables, Vector2Int position,
            float radius)
        {
            var viewRadiusSq = radius * radius;
            var viewArea = ViewCalculator.FieldOfView(position, _tilemap.Size, pos => passables.Contains(pos));
            return viewArea.Where(x => (x - position).sqrMagnitude <= viewRadiusSq).ToHashSet();
        }
    }
}