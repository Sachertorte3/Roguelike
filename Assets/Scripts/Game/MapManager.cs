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
        private List<IEventArea> _rooms = new();
        private MonsterHouse? _monsterHouse;
        private Shop? _shop;
        public IShop? Shop => _shop;
        public ReadOnlyReactiveProperty<bool>? IsStolen => _shop?.IsStolen;
        public RectInt? ShopRect => _shop?.Rect;
        private ReactiveProperty<bool> _stairsLocked = new(true);
        public ReadOnlyReactiveProperty<bool> MovementEntityLocked => _stairsLocked;
        public ObservableList<ICharacter> KeyCharacters = new();
        private readonly Subject<OnEffectSpawnedMessage> _onEffectSpawned = new();
        private readonly IGameManager _gameManager;
        public EntityManager EntityManager { get; init; }

        public MapManager(MapMemento map, DungeonMapData data, PlayerMemento? playerData,
            List<CharacterMemento>? partyMembers,
            Vector2Int? playerPosition, bool resetPertyPositions, IGameManager gameManager, CharacterControlInputReceiver receiver, ItemPlaceholders itemPlaceholders)
        {
            Id = map.Id;
            ItemPlaceholders = itemPlaceholders;
            _gameManager = gameManager;

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

            EntityManager = new EntityManager(map.Entities, playerData, partyMembers, playerPosition.Value, resetPertyPositions, receiver, gameManager, this);

            _dungeonData = data;

            if (map.MonsterHouse.HasValue)
            {
                _monsterHouse = new MonsterHouse(map.MonsterHouse.Value, EntityManager.Player.Character.Entity.CurrentPosition);
                _rooms.Add(_monsterHouse);
            }

            if (map.Shop.HasValue)
            {
                var clerk = EntityManager.Characters.FirstOrDefault(character =>
                    character.Entity.Id == map.Shop.Value.ClerkId);
                if (clerk == null && !map.Shop.Value.IsStolen)
                {
                    var clerkPosition = GetAllBlankPositionsOn(EntityLayer.Middle)
                        .In(map.Shop.Value.Room.Room.RectRange())
                        .GetAtRandom();
                    clerk = EntityManager.SpawnCharacter(
                        CharacterFactory.BuildCharacter(_dungeonData.Clerk, clerkPosition.Position,
                            homeLocation: new Location(Id, clerkPosition.Position)),
                            gameManager,
                            this);
                }

                if (clerk != null)
                {
                    _shop = new Shop(map.Shop.Value, clerk, gameManager, this);
                    EntityManager.AddClerk(_shop.Clerk);
                    _rooms.Add(_shop);
                }
            }

            SetRules(gameManager);

            KeyCharacters = new ObservableList<ICharacter>(map.KeyCharacters
                .Select(character => EntityManager.Characters.ById(new Id<IEntity>(character)))
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

            var visibleArea = EntityManager.Player.Character.VisionRange.VisibleArea;
            _tilemap.UpdateChunk(EntityManager.Player.Character.Entity.CurrentPosition);
            _tilemap.SetTilesKnown(visibleArea, true);

            UpdateVisibility(EntityManager.Entities);

            if (map.MonsterHouse.HasValue && !map.MonsterHouse.Value.HasEverEntered)
            {
                GameLog.AddIgnoreVisibility("<color=yellow>不穏な気配を感じる……</color>");
            }
        }

        public Observable<OnEffectSpawnedMessage> OnEffectSpawned => _onEffectSpawned;

        public void Dispose()
        {
            EntityManager.Dispose();
            _disposables.Dispose();
        }

        public IItemEntity SpawnItem(IItem item, Vector2Int position)
        {
            return EntityManager.SpawnItem(item,
                FindBlankPositionFrom(position, position => At(position).IsBlankAndStandable(EntityLayer.Bottom)));
        }

        public ICharacter SpawnEnemy(EnemyData enemy, Vector2Int position, IAffiliation? affiliation = null,
            bool? isSlept = null, bool? isShiny = null)
        {
            return EntityManager.SpawnCharacter(
                CharacterFactory.BuildCharacter(
                    enemy,
                    FindBlankPositionFrom(position, position => At(position).IsBlankAndStandable(EntityLayer.Middle)),
                    isSlept: isSlept ?? RandUtils.IsLessThanProbability(_dungeonData.SleepChance),
                    isShiny: isShiny ?? RandUtils.IsLessThanProbability(_dungeonData.ShinyChance),
                    affiliation: affiliation
                ),
                _gameManager,
                this
            );
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
            return await EntityManager.ShowThrowAnimation(icon, position, direction, distance, this, canHitLayer);
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
                visibility = EntityManager.Player.Character.IsVisible(entity.Entity.CurrentPosition);
            entity.Entity.SetVisibility(visibility);
        }

        public bool IsGrass(Vector2Int position)
        {
            return TilemapViewer.IsGrass(position);
        }

        public async UniTask ExecuteTrapAt(Vector2Int position, ICharacter actor)
        {
            var eventEntities = EntityManager.GetEventEntityAt(position, EntityLayer.Bottom);
            foreach (var eventEntity in eventEntities)
            {
                if (eventEntity is Trap trapEntity)
                {
                    await trapEntity.Event.DoEvent(actor, _gameManager, this);
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
                EntityManager.Entities
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

        public ITilemapViewer TilemapViewer => _tilemap;

        public MapMemento Serialize()
        {
            var characters = EntityManager.Characters.ToList();
            characters.Remove(EntityManager.Player.Character);
            return new MapMemento
            (
                Id,
                _tilemap.Serialize(),
                EntityManager.Serialize(),
                KeyCharacters.Select(character => character.Entity.Id.ToString()).ToList(),
                _monsterHouse.ToOption().Map(x => x.Serialize()),
                _shop.ToOption().Map(x => x.Serialize()),
                GetRandomBlankPositionOn(EntityLayer.Bottom, EntityLayer.Middle, EntityLayer.Top).Position
            );
        }

        public MapMemento SerializeWithoutPartyMembers()
        {
            return new MapMemento
            (
                Id,
                _tilemap.Serialize(),
                EntityManager.SerializeWithoutPartyMembers(GetFollowingCharacters()),
                KeyCharacters.Select(character => character.Entity.Id.ToString()).ToList(),
                _monsterHouse.ToOption().Map(x => x.Serialize()),
                _shop.ToOption().Map(x => x.Serialize()),
                GetRandomBlankPositionOn(EntityLayer.Bottom, EntityLayer.Middle, EntityLayer.Top).Position
            );
        }

        private void SetRules(IGameManager gameManager)
        {
            EntityManager.Characters.SubscribeIncludingCurrentObservables(
                character => character.OnDead,
                (character, _) => { DropAllItem(character); }
            ).AddTo(_disposables);

            EntityManager.Player.Character.VisionRange.OnVisibleAreaChanged.Subscribe(areaChanged =>
            {
                _tilemap.SetTilesKnown(EntityManager.Player.Character.VisionRange.VisibleArea, true);
                UpdateVisibility(EntityManager.Entities);
            }).AddTo(_disposables);

            EntityManager.Player.Character.Entity.Position.Subscribe(async positionChanged =>
            {
                _tilemap.UpdateChunk(positionChanged);
                if (IsGrass(positionChanged))
                {
                    Log.Debug($"SetGrasses: {EntityManager.Player.Character.Entity.CurrentPosition}");
                    SetGrasses(new[] { EntityManager.Player.Character.Entity.CurrentPosition }, false);
                    _gameManager.PlaySE(SE.GrassWalk);
                }
                var eventId = gameManager.StartEvent();
                foreach (var eventArea in _rooms)
                {
                    await eventArea.UpdatePosition(_gameManager, this, positionChanged);
                }
                gameManager.EndEvent(eventId);

            }).AddTo(_disposables);

            EntityManager.Characters.SubscribeIncludingCurrentObservables(
                character => character.Entity.Position.SkipLatestValueOnSubscribe(),
                async (character, positionChanged) =>
                {
                    var item = EntityManager.GetItemAt(positionChanged);
                    if (item != null)
                    {
                        if (character.CanPickUp
                            && character.CanPickUpItem()
                            && EntityManager.CanPickUpAt(positionChanged,
                                character.IsPlayer && Settings.GlobalSettings.AutoPickUpShopItem.CurrentValue))
                        {
                            EntityManager.PickUpAt(positionChanged,
                                character.IsPlayer && Settings.GlobalSettings.AutoPickUpShopItem.CurrentValue);
                            if (character.TryAddToInventory(item.Item))
                            {
                                if (EntityManager.Player.Character.IsVisible(positionChanged))
                                {
                                    GameLog.Add(character.Entity.IsVisible,
                                        $"{character.GetName(EntityManager.Player)}は<color=yellow>{item.Item.GetName(EntityManager.Player, ItemPlaceholders)}</color>を拾った");
                                }
                            }
                            else
                            {
                                throw new Exception("Unexpected error. Unable to pick up item.");
                            }
                        }
                        else if (EntityManager.Player.Character.IsVisible(positionChanged))
                        {
                            GameLog.Add(character.Entity.IsVisible,
                                $"{character.GetName(EntityManager.Player)}は<color=yellow>{item.Item.GetName(EntityManager.Player, ItemPlaceholders)}</color>の上に乗った");
                        }
                    }
                    var eventId = gameManager.StartEvent();
                    var eventEntities = EntityManager.GetEventEntityAt(positionChanged, EntityLayer.Bottom);
                    foreach (var eventEntity in eventEntities)
                    {
                        await eventEntity.Event.DoEvent(character, _gameManager, this);
                    }

                    if (character.IsPlayer)
                    {
                        var playerEventEntities = EntityManager.GetPlayerEventEntityAt(positionChanged, EntityLayer.Bottom);
                        await playerEventEntities.Select(entity => entity.Event).ToList().DoEvent(EntityManager.Player, _gameManager, this);
                    }
                    gameManager.EndEvent(eventId);
                }
            ).AddTo(_disposables);

            EntityManager.Characters.SubscribeIncludingCurrentObservables(
                character => character.Status.GetFlagProperty(FlagStatType.IsAffectedByTrap).SkipLatestValueOnSubscribe(),
                async (character, affectedByTrap) =>
                {
                    var eventId = gameManager.StartEvent();
                    if (affectedByTrap)
                    {
                        await ExecuteTrapAt(character.Entity.CurrentPosition, character);
                    }
                    gameManager.EndEvent(eventId);
                }
            ).AddTo(_disposables);

            EntityManager.Entities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.Position,
                (entity, _) => UpdateVisibility(entity)
            ).AddTo(_disposables);

            _tilemap.OnOverlayTilesChanged.Subscribe(overlayTilesChanged =>
            {
                foreach (var (position, category) in overlayTilesChanged)
                {
                    var entity = EntityManager.Entities.At(position);
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
                    EntityManager.Characters.In(visibleArea).ForEach(character => character.VisionRange.Refresh());
                }
            }).AddTo(_disposables);

            EntityManager.SetRules(gameManager);
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
                    .Except(EntityManager.Player.Character.VisionRange.VisibleArea);
                if (positions.Any())
                    SpawnRandomEnemy(positions.GetAtRandom(), null, false);
            }

            var unloadedCharacters = EntityManager.Characters
                .Where(character => !_tilemap.IsPositionInsideActiveChunk(character.Entity.CurrentPosition))
                .ToList();
            foreach (var character in unloadedCharacters)
            {
                EntityManager.RemoveCharacter(character);
            }

            EntityManager.UpdateTurn(this);

            SetGrasses(EntityManager.FireEntities.Positions(), false);

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

        public void DropAllItem(ICharacter character)
        {
            foreach (var item in character.ClearInventory())
            {
                SpawnItem(item,
                    FindBlankPositionFrom(character.Entity.CurrentPosition,
                        position => At(position).IsBlankAndStandable(EntityLayer.Bottom)));
            }
        }

        public void DropAllItemInStorage(IItem storageItem)
        {
            if (storageItem.ItemStorage.IsNone)
                return;
            var position = EntityManager.GetItemPositionByIdFromWorldOrInventory(storageItem.Id);
            if (!position.HasValue)
                throw new Exception("Item not found in world or inventory");
            foreach (var item in storageItem.ItemStorage.Expect("ItemStorage is null").Clear())
            {
                SpawnItem(item, FindBlankPositionFrom(position.Value, position => At(position).IsBlankAndStandable(EntityLayer.Bottom)));
            }
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
            var viewArea = ViewCalculator.FieldOfView(position, Mathf.CeilToInt(radius), isTileBlocked);
            return viewArea.Where(x => (x - position).sqrMagnitude <= viewRadiusSq).ToHashSet();
        }

        public IPlayer Player => EntityManager?.Player;
        public IObservableCollection<IEntity> Entities => EntityManager.Entities;
        public IObservableCollection<ICharacter> Characters => EntityManager.Characters;
        public IObservableCollection<IItemEntity> Items => EntityManager.Items;
        public IObservableCollection<ThrowAnimationEntity> ThrowAnimationEntities => EntityManager.ThrowAnimationEntities;
        public IObservableCollection<Fire> FireEntities => EntityManager.FireEntities;
        public IObservableCollection<IEventEntity> EventEntities => EntityManager.EventEntities;
        public IObservableCollection<IPlayerEventEntity> PlayerEventEntities => EntityManager.PlayerEventEntities;
        public IObservableCollection<IScheduledEventEntity> ScheduledEventEntities => EntityManager.ScheduledEventEntities;
        public IObservableCollection<IEventEntity> StandaloneEventEntities => EntityManager.StandaloneEventEntities;
        public IObservableCollection<IPlayerEventEntity> StandalonePlayerEventEntities => EntityManager.StandalonePlayerEventEntities;
        public IObservableCollection<IScheduledEventEntity> StandaloneScheduledEventEntities => EntityManager.StandaloneScheduledEventEntities;
        public List<Stairs> Stairs => EntityManager.Stairs;
        public IEntity? GetEntityFastAt(Vector2Int position, EntityLayer layer) => EntityManager.GetEntityFastAt(position, layer);
        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers) => EntityManager.GetEntitiesFastAt(position, layers);
        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position) => EntityManager.GetEntitiesFastAt(position);
        public IItem? GetItemByIdFromWorldOrInventory(Id<IItem> id) => EntityManager.GetItemByIdFromWorldOrInventory(id);
        public HashSet<Vector2Int> AllCharacterPositionsFast() => EntityManager.AllCharacterPositionsFast();
        public HashSet<Vector2Int> AllItemPositionsFast() => EntityManager.AllItemPositionsFast();
        public void AttackStatue(IEnumerable<Vector2Int> positions) => EntityManager.AttackStatue(positions);
        public void SpawnFire(IEnumerable<Vector2Int> positions)
        {
            foreach (var position in positions)
            {
                if (At(position).CanPlace(false, false, true))
                    EntityManager.SpawnFire(position);
            }
        }
        public IItemEntity? TryPickUpAt(Vector2Int position, bool canPickUpShopItem) => EntityManager.TryPickUpAt(position, canPickUpShopItem);
        public IEnumerable<ICharacter> GetFollowingCharacters() => EntityManager.GetFollowingCharacters();
    }
}