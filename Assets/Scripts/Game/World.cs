#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using VContainer;

namespace Game
{
    public class World : ISerializable<WorldMemento>
    {
        private ReactiveProperty<MapManager?> _activeMap = new();
        private Location _activeLocation;
        private Dungeon _activeDungeon => _dungeons[_activeLocation.MapName];
        private Id<IMap> _activeMapId => GetMapId(_activeLocation);
        private Dictionary<Id<IMap>, MapMemento> _maps = new();
        private HashSet<Id<IMap>> _updatedMapIds = new();
        private CharacterControlInputReceiver _receiver;
        private Dictionary<string, Dungeon> _dungeons = new();

        [Inject]
        public World(CharacterControlInputReceiver receiver)
        {
            Globals.World = this;
            _receiver = receiver;
        }

        public void CreateNew(DungeonBluePrintData dungeonData)
        {
            _dungeons[dungeonData.name] = new Dungeon(Dungeon.Build(dungeonData));
            _maps = new Dictionary<Id<IMap>, MapMemento>();
            _updatedMapIds = new HashSet<Id<IMap>>();
            _activeMap.Value = null;
        }

        public MapManager LoadWorld(WorldMemento memento, List<(string, MapMemento)> maps)
        {
            _dungeons = memento.Dungeons.ToDictionary(dungeon => dungeon.Key, dungeon => new Dungeon(dungeon.Value));
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
                _dungeons[memento.CurrentLocation.MapName].CreateMapData(memento.CurrentLocation.Level), memento.Player,
                new List<CharacterMemento>(), memento.Player.Entity.Position, _receiver, memento.CurrentLocation.Level);

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
                playerData,
                _maps.Select(map => map.Key.ToString()).ToList(),
                _activeLocation
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
            Id<IEntity>? upStairsId = null;
            Id<IEntity>? upStairsDestinationId = null;
            Id<IEntity>? downStairsId = null;
            Id<IEntity>? downStairsDestinationId = null;
            if (_dungeons[location.MapName].ExistLevel(location.Level - 1))
            {
                var prevMapId = _dungeons[location.MapName].GetMapId(location.Level - 1);
                if (_maps.ContainsKey(prevMapId))
                {
                    var prevMap = _maps[prevMapId];
                    var downStairs =
                        prevMap.EventEntities.Stairs.First(stairs => stairs.Type == MovementEntityType.DownStairs);
                    upStairsId = downStairs.DestinationId;
                    upStairsDestinationId = new Id<IEntity>(downStairs.Entity.Id);
                }
            }

            if (_dungeons[location.MapName].ExistLevel(location.Level + 1))
            {
                var nextMapId = _dungeons[location.MapName].GetMapId(location.Level + 1);
                if (_maps.ContainsKey(nextMapId))
                {
                    var nextMap = _maps[nextMapId];
                    var upStairs =
                        nextMap.EventEntities.Stairs.First(stairs => stairs.Type == MovementEntityType.UpStairs);
                    downStairsId = upStairs.DestinationId;
                    downStairsDestinationId = new Id<IEntity>(upStairs.Entity.Id);
                }
            }

            return _dungeons[location.MapName].CreateMapManager(id, location.Level, upStairsId,
                upStairsDestinationId, downStairsId, downStairsDestinationId);
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
                characters, initialPosition, _receiver, location.Level);

            _activeLocation = location;
            _activeMap.Value = map;
            return map;
        }
    }
}