#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Message;
using Domain.Model.Character.Status;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Characters;
using Domain.Service.Characters.Behavior;
using Domain.Service.Effect;
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Logs;
using Domain.Service.Map;
using Domain.Service.Rooms;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Game
{
    public class MapManager : IDisposable, ISerializable<MapMemento>, IMap
    {
        public Id<IMap> Id { get; init; }
        public string Name => "Dungeon";
        public int Depth => _dungeonData.Depth;
        public MapType Type => _dungeonData.Type;
        public ItemDatabase ItemDatabase => _dungeonData.ItemDatabase;
        public ItemPlaceholders ItemPlaceholders { get; init; }
        private readonly CompositeDisposable _disposables = new();
        private readonly ITilemap _tilemap;
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

        public MapManager(MapMemento map, DungeonMapData data, PlayerMemento? playerData,
            List<CharacterMemento>? partyMembers,
            Vector2Int? playerPosition, CharacterControlInputReceiver receiver, ItemPlaceholders itemPlaceholders)
        {
            Id = map.Id;
            ItemPlaceholders = itemPlaceholders;

            if (playerPosition == null)
            {
                playerPosition = map.RandomBlankPosition;
            }

            _tilemap = new Tilemap(map.Tilemap);

            if (playerData == null)
            {
                playerData = CharacterFactory.BuildPlayer("Player", playerPosition.Value);
            }
            else
            {
                playerData = playerData.CopyWith(character: playerData.Character.ReplacePosition(playerPosition.Value));
            }

            CharacterManager = new CharacterManager(playerData, receiver, this);
            ItemManager = new ItemManager();
            EventEntityManager = new EventEntityManager(map.EventEntities, _stairsLocked);
            ThrowAnimationEntityManager = new ThrowAnimationEntityManager();
            FireEntityManager = new FireEntityManager(map.Fires);
            _entities.AddWith(Characters).AddTo(_disposables);
            _entities.AddWith(Items).AddTo(_disposables);
            _entities.AddWith(EventEntities).AddTo(_disposables);
            _entities.AddWith(PlayerEventEntities).AddTo(_disposables);
            _entities.AddWith(ThrowAnimationEntities).AddTo(_disposables);
            _entities.AddWith(FireEntities).AddTo(_disposables);

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
                _monsterHouse = new MonsterHouse(map.MonsterHouse.Value, Player.Character.Entity.CurrentPosition);
                _eventAreas.Add(_monsterHouse);
            }

            if (map.Shop.HasValue)
            {
                var clerk = Characters.FirstOrDefault(character =>
                    character.Entity.Id == map.Shop.Value.ClerkId);
                if (clerk == null && !map.Shop.Value.IsStolen)
                {
                    var clerkPosition = GetAllBlankPositionsOn(EntityLayer.Middle)
                        .In(map.Shop.Value.Room.Room.RectRange())
                        .GetAtRandom();
                    var ally = CharacterManager.SpawnAlly(
                        CharacterFactory.BuildCharacter(_dungeonData.Clerk, clerkPosition.Position,
                            homeLocation: new Location(Id, clerkPosition.Position)),
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
                .Select(character => CharacterManager.Characters.ById(new Id<IEntity>(character)))
                .WhereNotNull()
            );
            if (KeyCharacters.Any())
            {
                KeyCharacters.ForEach(character =>
                    character.Entity.OnDestroyed.Subscribe(_ => KeyCharacters.Remove(character)).AddTo(_disposables));
                KeyCharacters.ObserveCountChanged().Where(count => count == 0)
                    .Subscribe(_ => _stairsLocked.Value = false).AddTo(_disposables);
            }
            else
            {
                _stairsLocked.Value = false;
            }

            var visibleArea = CharacterManager.Player.Character.VisionRange.VisibleArea;
            _tilemap.UpdateChunk(CharacterManager.Player.Character.Entity.CurrentPosition);
            _tilemap.SetTilesKnown(visibleArea, true);

            UpdateVisibility(Entities);

            if ((map.MonsterHouse.HasValue && !map.MonsterHouse.Value.HasEverEntered)
                || map.Characters.Any(character => character.IsShiny))
            {
                GameLog.Add("<color=yellow>不穏な気配を感じる……</color>");
            }
        }

        public IPlayer Player => CharacterManager?.Player;

        public CharacterManager CharacterManager { get; init; }
        public ItemManager ItemManager { get; init; }
        public EventEntityManager EventEntityManager { get; init; }
        public ThrowAnimationEntityManager ThrowAnimationEntityManager { get; init; }
        public FireEntityManager FireEntityManager { get; init; }

        public IObservableCollection<ICharacter> Characters => CharacterManager.Characters;
        public IObservableCollection<IItemEntity> Items => ItemManager.Items;
        public IObservableCollection<IEventEntity> EventEntities => EventEntityManager.EventEntities;
        public IObservableCollection<IPlayerEventEntity> PlayerEventEntities => EventEntityManager.PlayerEventEntities;

        public IObservableCollection<ThrowAnimationEntity> ThrowAnimationEntities =>
            ThrowAnimationEntityManager.ThrowAnimationEntities;

        public IObservableCollection<Fire> FireEntities => FireEntityManager.FireEntities;

        public Observable<OnEffectSpawnedMessage> OnEffectSpawned => _onEffectSpawned;

        public void Dispose()
        {
            CharacterManager.Dispose();
            ItemManager.Dispose();
            EventEntities.ForEach(eventEntity => eventEntity.Dispose());
            PlayerEventEntities.ForEach(eventEntity => eventEntity.Dispose());
            ThrowAnimationEntities.ForEach(throwAnimationEntity => throwAnimationEntity.Dispose());
            FireEntities.ForEach(fireEntity => fireEntity.Dispose());
            _disposables.Dispose();
        }

        public class EntityIdComparer : IEqualityComparer<IEntity>
        {
            public bool Equals(IEntity? x, IEntity? y)
            {
                return x?.Entity.Id == y?.Entity.Id;
            }

            public int GetHashCode(IEntity obj)
            {
                return obj.Entity.Id.GetHashCode();
            }
        }

        private readonly ObservableList<IEntity> _entities = new();
        public IEnumerable<IEntity> Entities => _entities.Distinct(new EntityIdComparer());

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
                    isSlept: isSlept ?? RandUtils.IsLessThanProbability(_dungeonData.SleepChance),
                    isShiny: isShiny ?? RandUtils.IsLessThanProbability(_dungeonData.ShinyChance),
                    affiliation: affiliation
                ),
                this
            );
            EventEntityManager.Add(ally);
            return ally.Character;
        }

        public ICharacter? SpawnRandomEnemy(Vector2Int position, bool? isSlept = null, bool? isShiny = null)
        {
            if (_dungeonData.Enemies.Count == 0)
                return null;
            return SpawnEnemy(_dungeonData.Enemies.GetRandomItem(), position, isSlept: isSlept, isShiny: isShiny);
        }

        public async UniTask<Vector2Int> ShowThrowAnimation(Sprite icon, Vector2Int position, Direction8 direction,
            int distance, params EntityLayer[] canHitLayer)
        {
            var throwAnimationEntity = new ThrowAnimationEntity(position, icon);
            ThrowAnimationEntityManager.Add(throwAnimationEntity);
            var destination = await throwAnimationEntity.Throw(direction, this, distance, canHitLayer);
            throwAnimationEntity.Entity.Destroy("は演出が終わったので消えた（エラー）");
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

        public void UpdateVisibility(IEnumerable<IEntity> entities)
        {
            foreach (var entity in entities)
                UpdateVisibility(entity);
        }

        public void UpdateVisibility(IEntity entity)
        {
            bool visibility;
            if (IsGrass(entity.Entity.CurrentPosition) && entity.Entity.Layer == EntityLayer.Bottom)
                visibility = false;
            else
                visibility = Player.Character.IsVisible(entity.Entity.CurrentPosition);
            entity.Entity.SetVisibility(visibility);
        }

        public IItem? GetItemByIdFromWorldOrInventory(Id<IItem> id)
        {
            var itemEntity = ItemManager.Items.ById(id);
            if (itemEntity != null)
                return itemEntity.Item;
            foreach (var character in Characters)
            {
                var item = character.Inventory.AllItems.ById(id);
                if (item != null)
                    return item;
            }

            return null;
        }

        public List<IEventEntity> GetEventEntityAt(Vector2Int position, EntityLayer layer)
        {
            return EventEntities
                .Where(eventEntity => eventEntity.Entity.CurrentPosition == position)
                .Where(eventEntity => eventEntity.Entity.Layer == layer)
                .ToList();
        }

        public List<IPlayerEventEntity> GetPlayerEventEntityAt(Vector2Int position, EntityLayer layer)
        {
            return PlayerEventEntities
                .Where(eventEntity => eventEntity.Entity.CurrentPosition == position)
                .Where(eventEntity => eventEntity.Entity.Layer == layer)
                .ToList();
        }

        public bool IsGrass(Vector2Int position)
        {
            return TilemapViewer.IsGrass(position);
        }

        public bool IsFireAt(Vector2Int position)
        {
            return FireEntities.Any(fire => fire.Entity.CurrentPosition == position);
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

        public IMapPosition? GetRandomBlankPositionOn(params EntityLayer[] layers)
        {
            foreach (var position in TilemapViewer.GetAllPassablePositions().Shuffled())
            {
                var mapPosition = At(position);
                if (mapPosition.IsBlank(layers))
                    return mapPosition;
            }

            return null;
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

        public void RemoveEventEntity(IPlayerEventEntity eventEntity)
        {
            EventEntityManager.Remove(eventEntity);
        }

        public ITilemapViewer TilemapViewer => _tilemap;

        public MapMemento Serialize()
        {
            var characters = Characters.ToList();
            characters.Remove(Player.Character);
            return new MapMemento
            (
                Id,
                _tilemap.Serialize(),
                characters.Select(character => character.Serialize()).ToList(),
                ItemManager.Items.Select(item => item.Serialize()).ToList(),
                EventEntityManager.Serialize(),
                FireEntityManager.Serialize(),
                KeyCharacters.Select(character => character.Entity.Id.ToString()).ToList(),
                _monsterHouse.ToOption().Map(x => x.Serialize()),
                _shop.ToOption().Map(x => x.Serialize()),
                GetRandomBlankPositionOn(EntityLayer.Bottom, EntityLayer.Middle, EntityLayer.Top).Position
            );
        }

        public MapMemento SerializeWithoutPartyMembers()
        {
            var characters = Characters.ToList();
            characters.Remove(Player.Character);
            characters.RemoveAll(character => GetFollowingCharacters().Contains(character));
            return new MapMemento
            (
                Id,
                _tilemap.Serialize(),
                characters.Select(character => character.Serialize()).ToList(),
                ItemManager.Items.Select(item => item.Serialize()).ToList(),
                EventEntityManager.Serialize(),
                FireEntityManager.Serialize(),
                KeyCharacters.Select(character => character.Entity.Id.ToString()).ToList(),
                _monsterHouse.ToOption().Map(x => x.Serialize()),
                _shop.ToOption().Map(x => x.Serialize()),
                GetRandomBlankPositionOn(EntityLayer.Bottom, EntityLayer.Middle, EntityLayer.Top).Position
            );
        }

        private void SetRules()
        {
            Characters.SubscribeIncludingCurrentObservables(
                character => character.OnDead,
                (character, _) => { DropAllItem(character); }
            ).AddTo(_disposables);

            Player.Character.VisionRange.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                _tilemap.SetTilesKnown(Player.Character.VisionRange.VisibleArea, true);
                UpdateVisibility(Entities);
            }).AddTo(_disposables);

            Player.Character.Entity.Position.Subscribe(async positionChanged =>
            {
                _tilemap.UpdateChunk(positionChanged);
                SetGrasses(new[] { Player.Character.Entity.CurrentPosition }, false);
                EventExecutionCount++;
                foreach (var eventArea in _eventAreas)
                {
                    await eventArea.UpdatePosition(Globals.GameManager, this, positionChanged);
                }

                EventExecutionCount--;
            }).AddTo(_disposables);

            Characters.SubscribeIncludingCurrentObservables(
                character => character.Entity.Position.SkipLatestValueOnSubscribe(),
                async (character, positionChanged) =>
                {
                    var item = ItemManager.GetItemAt(positionChanged);
                    if (item != null)
                    {
                        if (character.CanPickUp
                            && character.CanPickUpItem()
                            && ItemManager.CanPickUpAt(positionChanged,
                                character.IsPlayer && Settings.GlobalSettings.AutoPickUpShopItem.CurrentValue))
                        {
                            ItemManager.PickUpAt(positionChanged,
                                character.IsPlayer && Settings.GlobalSettings.AutoPickUpShopItem.CurrentValue);
                            if (character.TryAddToInventory(item.Item))
                            {
                                if (Player.Character.IsVisible(positionChanged))
                                {
                                    GameLog.Add(
                                        $"{character.GetName(Player)}は<color=yellow>{item.Item.GetName(Player, ItemPlaceholders)}</color>を拾った");
                                }
                            }
                            else
                            {
                                throw new Exception("Unexpected error. Unable to pick up item.");
                            }
                        }
                        else if (Player.Character.IsVisible(positionChanged))
                        {
                            GameLog.Add(
                                $"{character.GetName(Player)}は<color=yellow>{item.Item.GetName(Player, ItemPlaceholders)}</color>の上に乗った");
                        }
                    }

                    EventExecutionCount++;
                    var eventEntities = GetEventEntityAt(positionChanged, EntityLayer.Bottom);
                    foreach (var eventEntity in eventEntities)
                    {
                        await eventEntity.Event.DoEvent(character, Globals.GameManager, this);
                    }

                    if (character.IsPlayer)
                    {
                        var playerEventEntities = GetPlayerEventEntityAt(positionChanged, EntityLayer.Bottom);
                        foreach (var playerEventEntity in playerEventEntities)
                        {
                            await playerEventEntity.Event.DoEvent(Player, Globals.GameManager, this);
                        }
                    }

                    EventExecutionCount--;
                }
            ).AddTo(_disposables);

            Characters.SubscribeIncludingCurrentObservables(
                character => character.Status.GetFlagProperty(FlagStatType.IsAffectedByTrap).SkipLatestValueOnSubscribe(),
                async (character, affectedByTrap) =>
                {
                    EventExecutionCount++;
                    if (affectedByTrap)
                    {
                        await ExecuteTrapAt(character.Entity.CurrentPosition, character);
                    }

                    EventExecutionCount--;
                }
            ).AddTo(_disposables);

            _entities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.Position,
                (entity, _) => UpdateVisibility(entity)
            ).AddTo(_disposables);

            _tilemap.OnOverlayTilesChanged.Subscribe(overlayTilesChanged =>
            {
                foreach (var (position, category) in overlayTilesChanged)
                {
                    var entity = Entities.At(position);
                    foreach (var e in entity)
                    {
                        if (e.Entity.Layer == EntityLayer.Bottom)
                            UpdateVisibility(e);
                    }
                }
            }).AddTo(_disposables);

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
            if (RandUtils.IsLessThanProbability(CommonSenseParameters.SpawnEnemyProbabilityPerTurn))
            {
                var positions = GetAllBlankPositionsOn(EntityLayer.Middle).Values()
                    .Except(Player.Character.VisionRange.VisibleArea);
                if (positions.Any())
                    SpawnRandomEnemy(positions.GetAtRandom(), null, false);
            }

            var unloadedCharacters = Characters
                .Where(character => !_tilemap.IsPositionInsideActiveChunk(character.Entity.CurrentPosition))
                .ToList();
            foreach (var character in unloadedCharacters)
            {
                CharacterManager.RemoveCharacter(character);
            }

            FireEntityManager.UpdateTurn(this);

            var characters = Characters.In(FireEntityManager.FireEntities.Positions()).ToList();
            foreach (var character in characters)
            {
                character.LoseHp(1, "は火に焼かれた");
                GameLog.Add($"{character.GetName(Player)}は火に焼かれた");
            }

            var items = Items.In(FireEntityManager.FireEntities.Positions()).ToList();
            foreach (var item in items)
            {
                item.Entity.Destroy($"は灰になった");
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

        public IItemEntity? TryPickUpAt(Vector2Int position, bool canPickUpShopItem)
        {
            return ItemManager.TryPickUpAt(position, canPickUpShopItem);
        }

        public void DropAllItem(ICharacter character)
        {
            foreach (var item in character.ClearInventory())
            {
                SpawnItem(item,
                    FindBlankPositionFrom(character.Entity.CurrentPosition,
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
                .Where(character => !character.IsPlayer)
                .Where(character => character.IsAlly(Player.Character))
                .Where(character => character.IsVisible(Player.Character.Entity.CurrentPosition));
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
            if (_visionCache.TryGetValue(to, out area))
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
            _visionCache[from] = ViewCalculator.FieldOfView(from, 20, pos => !At(pos).IsLightPassable());
        }

        public HashSet<Vector2Int> ComputeCircle(Func<Vector2Int, bool> isTileBlocked, Vector2Int position,
            float radius)
        {
            var viewRadiusSq = radius * radius;
            var viewArea = ViewCalculator.FieldOfView(position, 20, isTileBlocked);
            return viewArea.Where(x => (x - position).sqrMagnitude <= viewRadiusSq).ToHashSet();
        }
    }
}