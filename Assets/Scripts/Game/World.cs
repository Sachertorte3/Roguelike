#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Dungeon;
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
        private Location _activeLocation;
        private Id<IMap> _activeMapId => GetMapId(_activeLocation);
        private Dictionary<Id<IMap>, MapMemento> _maps = new();
        private HashSet<Id<IMap>> _updatedMapIds = new();
        private Dictionary<string, Dungeon> _dungeons = new();
        private Dictionary<Location, List<MapConnection>> _movements = new();
        private ItemPlaceholders _itemPlaceholders;
        private Placeholders _placeholders;
        private CharacterControlInputReceiver _receiver;

        [Inject]
        public World(CharacterControlInputReceiver receiver)
        {
            Globals.World = this;
            _receiver = receiver;
            _placeholders = Addressables.LoadAssetAsync<Placeholders>("Assets/Database/Placeholders.asset").WaitForCompletion();
        }

        public void CreateNew()
        {
            var mainDungeon = Addressables.LoadAssetAsync<DungeonBluePrintData>("Assets/Database/DungeonBluePrintData/Dungeon.asset").WaitForCompletion();
            var voidDungeon = Addressables.LoadAssetAsync<DungeonBluePrintData>("Assets/Database/DungeonBluePrintData/Void.asset").WaitForCompletion();
            _dungeons = new Dictionary<string, Dungeon> {
                { "Dungeon", new Dungeon(Dungeon.Build(mainDungeon)) },
                { "Void", new Dungeon(Dungeon.Build(voidDungeon)) }
            };
            _itemPlaceholders = new ItemPlaceholders(ItemPlaceholders.Build(_placeholders), _placeholders);
            _movements = new Dictionary<Location, List<MapConnection>>();
            for (int i = 1; i <= 10; i++)
            {
                var movement = new MapConnection(MovementEntityType.MagicCircle, new Location("Void", 1));
                var reverse = new MapConnection(MovementEntityType.MagicCircle, new Location("Dungeon", i));
                AddMovement(movement, reverse);
            }
            _maps = new Dictionary<Id<IMap>, MapMemento>();
            _updatedMapIds = new HashSet<Id<IMap>>();
            _activeMap.Value = null;
        }

        public void AddMovement(MapConnection movement, MapConnection reverse)
        {
            if (!_movements.ContainsKey(reverse.Destination))
            {
                _movements[reverse.Destination] = new List<MapConnection>();
            }
            _movements[reverse.Destination].Add(movement);
            if (!_movements.ContainsKey(movement.Destination))
            {
                _movements[movement.Destination] = new List<MapConnection>();
            }
            _movements[movement.Destination].Add(reverse);
        }

        public DungeonMapData GetDungeonMapData(Location location)
        {
            return _dungeons[location.MapName].CreateMapData(location.Level);
        }

        public MapManager LoadWorld(WorldMemento memento, List<(string, MapMemento)> maps)
        {
            _dungeons = memento.Dungeons.ToDictionary(dungeon => dungeon.Key, dungeon => new Dungeon(dungeon.Value));
            _itemPlaceholders = new ItemPlaceholders(memento.ItemPlaceholders, _placeholders);
            _movements = memento.Movements;
            _maps = memento.MapIds.ToDictionary(
                mapId => new Id<IMap>(mapId),
                mapId => maps.First(map => map.Item1 == mapId).Item2
            );

            var mapId = GetMapId(memento.CurrentLocation);
            _updatedMapIds = new HashSet<Id<IMap>> { mapId };

            Log.Debug($"LoadMap mapId:{mapId}");
            var mapMemento = GetMapMemento(memento.CurrentLocation);

            if (_activeMap.CurrentValue != null)
            {
                _activeMap.CurrentValue.Dispose();
            }

            MapManager map = new(mapMemento,
                GetDungeonMapData(memento.CurrentLocation), memento.Player,
                new List<CharacterMemento>(), memento.Player.Entity.Position, _receiver, _itemPlaceholders);

            _activeLocation = memento.CurrentLocation;
            _activeMap.Value = map;

            return map;
        }

        public WorldMemento Serialize()
        {
            _maps[_activeMapId] = _activeMap.CurrentValue.Serialize();
            var playerData = _activeMap.CurrentValue.Player.Serialize();
            return new WorldMemento
            (
                _dungeons.ToDictionary(dungeon => dungeon.Key, dungeon => dungeon.Value.Serialize()),
                _movements,
                playerData,
                _maps.Select(map => map.Key.ToString()).ToList(),
                _activeLocation,
                _itemPlaceholders.Serialize()
            );
        }

        public List<MapMemento> SerializeUpdatedMaps()
        {
            var updatedMaps = _updatedMapIds.Select(mapId => _maps[mapId]).ToList();
            _updatedMapIds.Clear();
            _updatedMapIds.Add(_activeMapId);
            return updatedMaps;
        }

        public ReadOnlyReactiveProperty<MapManager?> ActiveMap => _activeMap;

        private Id<IMap> GetMapId(Location location)
        {
            return _dungeons[location.MapName].GetMapId(location.Level);
        }

        private MapMemento GetMapMemento(Location location)
        {
            var mapId = _dungeons[location.MapName].GetMapId(location.Level);
            if (!_maps.ContainsKey(mapId))
            {
                _maps[mapId] = CreateMap(location, mapId);
                _updatedMapIds.Add(mapId);
            }

            return _maps[mapId];
        }

        private MapMemento CreateMap(Location location, Id<IMap> id)
        {
            List<MovementData> movementData = new();
            var upStairsLocation = new Location(location.MapName, location.Level - 1);
            if (_dungeons[location.MapName].ExistLevel(location.Level - 1))
            {
                movementData.Add(CreateMovementData(MovementEntityType.UpStairs, location, upStairsLocation));
            }

            var downStairsLocation = new Location(location.MapName, location.Level + 1);
            if (_dungeons[location.MapName].ExistLevel(location.Level + 1))
            {
                movementData.Add(CreateMovementData(MovementEntityType.DownStairs, location, downStairsLocation));
            }

            if (_movements.ContainsKey(location))
            {
                foreach (var movement in _movements[location])
                {
                    movementData.Add(CreateMovementData(movement.Type, location, movement.Destination));
                }
            }

            return _dungeons[location.MapName].CreateMapManager(id, location.Level,
                movementData);
        }

        private MovementData CreateMovementData(MovementEntityType type, Location current, Location destination)
        {
            var mapId = _dungeons[destination.MapName].GetMapId(destination.Level);
            if (_maps.ContainsKey(mapId))
            {
                var map = _maps[mapId];
                var destinationEntity =
                    map.EventEntities.Stairs
                    .First(stairs => stairs.Destination == current);
                var id = destinationEntity.DestinationId;
                var destinationId = new Id<IEntity>(destinationEntity.Entity.Id);
                return new MovementData(type, destination, id, destinationId);
            }
            return new MovementData(type, destination, null, null);
        }

        public MapManager LoadMap(Location location, Id<IEntity>? destination)
        {
            Log.Debug($"LoadMap location:{location}");
            var mapMemento = GetMapMemento(location);
            _updatedMapIds.Add(GetMapId(location));

            CharacterMemento? playerData = null;
            List<CharacterMemento>? characters = null;
            Vector2Int? initialPosition = destination != null
                ? mapMemento.EventEntities.Stairs.First(stairs => stairs.Entity.Id == destination.ToString()).Entity
                    .Position
                : null;
            if (_activeMap.CurrentValue != null)
            {
                _maps[_activeMapId] = _activeMap.CurrentValue.SerializeWithoutPartyMembers();
                playerData = _activeMap.CurrentValue.Player.Serialize();
                characters = _activeMap.CurrentValue.GetFollowingCharacters().Select(character => character.Serialize())
                    .ToList();

                _activeMap.CurrentValue.Dispose();
            }

            MapManager map = new(mapMemento, _dungeons[location.MapName].CreateMapData(location.Level), playerData,
                characters, initialPosition, _receiver, _itemPlaceholders);

            _activeLocation = location;
            _activeMap.Value = map;
            return map;
        }
    }
    public record MovementData(MovementEntityType Type, Location Destination, Id<IEntity>? Id, Id<IEntity>? DestinationId);
}