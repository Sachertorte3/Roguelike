#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using R3;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using VContainer;

namespace Game
{
    public class World : ISerializable<WorldMemento>
    {
        private ReactiveProperty<MapManager?> _activeMap = new();
        private Id<IMap>? _activeMapId => _activeMap.CurrentValue?.Id;
        private Dictionary<Id<IMap>, MapMemento> _maps = new();
        private HashSet<Id<IMap>> _updatedMapIds = new();
        private Dungeon _dungeon;
        private ItemPlaceholders _itemPlaceholders;
        private Placeholders _placeholders;
        private CharacterControlInputReceiver _receiver;

        [Inject]
        public World(CharacterControlInputReceiver receiver)
        {
            _receiver = receiver;
            _placeholders = Addressables.LoadAssetAsync<Placeholders>("Assets/Database/ItemData/Placeholders.asset")
                .WaitForCompletion();
            _activeMap.SubscribeIncludingCurrentValueIgnoreNull(map =>
            {
                _updatedMapIds.Add(_activeMapId);
            });
        }

        public void CreateNew()
        {
            _dungeon = new Dungeon(Dungeon.Build());
            _itemPlaceholders = new ItemPlaceholders(ItemPlaceholders.Build(), _placeholders);
            _maps = new Dictionary<Id<IMap>, MapMemento>();
            _updatedMapIds = new HashSet<Id<IMap>>();
            _activeMap.Value = null;
        }

        public DungeonMapData GetDungeonMapData(Id<IMap> mapId)
        {
            return _dungeon.CreateMapData(mapId);
        }

        public MapManager LoadWorld(WorldMemento memento, Dictionary<Id<IMap>, MapMemento> maps, IGameManager gameManager)
        {
            _dungeon = new Dungeon(memento.Dungeon);
            _itemPlaceholders = new ItemPlaceholders(memento.ItemPlaceholders, _placeholders);
            _maps = memento.MapIds.ToDictionary(
                mapId => mapId,
                mapId => maps[mapId]
            );

            _updatedMapIds = new HashSet<Id<IMap>> { memento.CurrentMapId };

            Log.Debug($"LoadMap mapId:{memento.CurrentMapId}");
            var mapMemento = GetMapMemento(memento.CurrentMapId);

            if (_activeMap.CurrentValue != null)
            {
                _activeMap.CurrentValue.Dispose();
            }

            MapManager map = new(mapMemento,
                GetDungeonMapData(memento.CurrentMapId), memento.Player, memento.PartyMembers, memento.Player.Character.Entity.Position, false, gameManager, _receiver, _itemPlaceholders);

            _activeMap.Value = map;

            return map;
        }

        public WorldMemento Serialize()
        {
            var playerData = _activeMap.CurrentValue.Player.Serialize();
            var partyMembers = _activeMap.CurrentValue.GetFollowingCharacters().Select(character => character.Serialize()).ToList();
            return new WorldMemento
            (
                _dungeon.Serialize(),
                playerData,
                partyMembers,
                _activeMap.CurrentValue.Player.Character.IsDead,
                _maps.Keys.ToList(),
                _activeMapId,
                _itemPlaceholders.Serialize()
            );
        }

        public List<MapMemento> SerializeUpdatedMaps()
        {
            _maps[_activeMapId] = _activeMap.CurrentValue.SerializeWithoutPartyMembers();
            var updatedMaps = _updatedMapIds.Select(mapId => _maps[mapId]).ToList();
            _updatedMapIds.Clear();
            _updatedMapIds.Add(_activeMapId);
            return updatedMaps;
        }

        public ReadOnlyReactiveProperty<MapManager?> ActiveMap => _activeMap;

        private MapMemento GetMapMemento(Id<IMap> mapId)
        {
            if (!_maps.ContainsKey(mapId))
            {
                _maps[mapId] = CreateMap(mapId);
            }

            return _maps[mapId];
        }

        private MapMemento CreateMap(Id<IMap> id)
        {
            List<MovementData> movementData = new();
            var destinations = _dungeon.GetDestinations(id);
            foreach (var destination in destinations)
            {
                movementData.Add(CreateMovementData(destination.Type, id, destination.Destination));
            }

            return _dungeon.CreateMapManager(id, movementData);
        }

        private MovementData CreateMovementData(MovementEntityType type, Id<IMap> current, Id<IMap> destination)
        {
            if (_maps.ContainsKey(destination))
            {
                var map = _maps[destination];
                var destinationEntity =
                    map.Entities.EventEntities.Stairs
                        .First(stairs => stairs.Destination == current);
                var id = destinationEntity.DestinationId;
                var destinationId = new Id<IEntity>(destinationEntity.Entity.Id);
                return new MovementData(type, destination, id, destinationId);
            }

            return new MovementData(type, destination, null, null);
        }

        public MapManager LoadStartMap(IGameManager gameManager)
        {
            return LoadMap(_dungeon.StartMapId, null, gameManager);
        }

        public MapManager LoadMap(Id<IMap> mapId, Id<IEntity>? destination, IGameManager gameManager)
        {
            Log.Debug($"LoadMap mapId:{mapId}");
            var mapMemento = GetMapMemento(mapId);

            PlayerMemento? playerData = null;
            List<CharacterMemento>? partyMembers = null;
            Vector2Int? initialPosition = destination != null
                ? mapMemento.Entities.EventEntities.Stairs.First(stairs => stairs.Entity.Id == destination.ToString()).Entity
                    .Position
                : null;
            if (_activeMap.CurrentValue != null)
            {
                _maps[_activeMapId] = _activeMap.CurrentValue.SerializeWithoutPartyMembers();
                playerData = _activeMap.CurrentValue.Player.Serialize();
                partyMembers = _activeMap.CurrentValue.GetFollowingCharacters()
                    .Select(character => character.Serialize()).ToList();

                _activeMap.CurrentValue.Dispose();
            }

            MapManager map = new(mapMemento, _dungeon.CreateMapData(mapId), playerData,
                partyMembers, initialPosition, true, gameManager, _receiver, _itemPlaceholders);

            _activeMap.Value = map;

            return map;
        }
    }
}