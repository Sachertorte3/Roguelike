#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using Domain.Service.Map;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using VContainer;

namespace Model.Game
{
    public class World : ISerializable<WorldMemento>
    {
        private ReactiveProperty<MapManager?> _activeMap = new();
        private Location _activeLocation;
        private Dungeon _activeDungeon => _dungeons[_activeLocation.MapName];
        private Id<MapManager> _activeMapId => GetMapId(_activeLocation);
        private Dictionary<Id<MapManager>, MapMemento> _maps = new();
        private HashSet<Id<MapManager>> _updatedMapIds = new();
        private CharacterControlInputReceiver _receiver;
        private Dictionary<string, Dungeon> _dungeons = new();

        [Inject]
        public World(CharacterControlInputReceiver receiver, DungeonBluePrintData dungeonData)
        {
            Globals.World = this;
            _receiver = receiver;
            _dungeons[dungeonData.name] = new(Dungeon.Build(dungeonData));
        }

        public void CreateNew(DungeonBluePrintData dungeonData)
        {
            _dungeons[dungeonData.name] = new(Dungeon.Build(dungeonData));
            _maps = new();
            _updatedMapIds = new();
            _activeMap.Value = null;
        }

        public MapManager LoadWorld(WorldMemento memento)
        {
            _dungeons = memento.Dungeons.ToDictionary(dungeon => dungeon.Key, dungeon => new Dungeon(dungeon.Value));
            _maps = memento.Maps.ToDictionary(map => new Id<MapManager>(map.Key), map => map.Value);

            var mapId = GetMapId(memento.CurrentLocation);
            _updatedMapIds = new HashSet<Id<MapManager>> { mapId };

            Log.Debug($"LoadMap mapId:{mapId}");
            var mapMemento = GetMapMemento(memento.CurrentLocation);

            if (_activeMap.CurrentValue != null)
            {
                _activeMap.CurrentValue.Dispose();
            }

            MapManager map = new(mapMemento, _dungeons[memento.CurrentLocation.MapName].CreateMapData(memento.CurrentLocation.Level), memento.Player, new(), memento.Player.Entity.Position, _receiver, memento.CurrentLocation.Level);

            _activeLocation = memento.CurrentLocation;
            _activeMap.Value = map;

            return map;
        }

        public WorldMemento Serialize()
        {
            _maps[_activeMapId] = _activeMap.CurrentValue.Serialize();
            var playerData = _activeMap.CurrentValue.Player.Serialize();
            return new WorldMemento
            {
                Dungeons = new(_dungeons.ToDictionary(dungeon => dungeon.Key, dungeon => dungeon.Value.Serialize())),
                Player = playerData,
                Maps = new(_maps.ToDictionary(map => map.Key.Value, map => map.Value)),
                CurrentLocation = _activeLocation
            };
        }

        public ReadOnlyReactiveProperty<MapManager?> ActiveMap => _activeMap;

        private Id<MapManager> GetMapId(Location location)
        {
            return _dungeons[location.MapName].GetMapId(location.Level);
        }
        private MapMemento GetMapMemento(Location location)
        {
            var mapId = _dungeons[location.MapName].GetMapId(location.Level);
            if (!_maps.ContainsKey(mapId))
            {
                var dungeonData = _dungeons[location.MapName].CreateMapData(location.Level);
                _maps[mapId] = new MapBuilder(Tilemap.Build(dungeonData.Field), dungeonData, location.Level).Build();
            }
            return _maps[mapId];
        }

        public MapManager LoadMap(Location location)
        {
            Log.Debug($"LoadMap location:{location}");
            var mapMemento = GetMapMemento(location);
            _updatedMapIds.Add(GetMapId(location));

            CharacterMemento? playerData = null;
            List<CharacterMemento>? characters = null;
            Vector2Int initialPosition = mapMemento.EventEntities.UpStairs.Entity.Position;
            if (_activeMap.CurrentValue != null)
            {
                _maps[_activeMapId] = _activeMap.CurrentValue.Serialize();
                playerData = _activeMap.CurrentValue.Player.Serialize();
                characters = _activeMap.CurrentValue.GetFollowingCharacters().Select(character => character.Serialize())
                    .ToList();

                if (location.Level < _activeLocation.Level)
                {
                    initialPosition = mapMemento.EventEntities.DownStairs.Entity.Position;
                }
                else if (location.Level > _activeLocation.Level)
                {
                    initialPosition = mapMemento.EventEntities.UpStairs.Entity.Position;
                }

                _activeMap.CurrentValue.Dispose();
            }

            MapManager map = new(mapMemento, _dungeons[location.MapName].CreateMapData(location.Level), playerData, characters, initialPosition, _receiver, location.Level);

            _activeLocation = location;
            _activeMap.Value = map;
            return map;
        }

        public HashSet<ICharacter> GetCharactersInArea(HashSet<Vector2Int> area)
        {
            if (ActiveMap.CurrentValue == null)
                throw new InvalidOperationException("ActiveMap is null");
            return ActiveMap.CurrentValue.GetCharactersInArea(area);
        }

        public void HandleItemDrop(int inventoryIndex)
        {
            if (ActiveMap.CurrentValue == null)
                throw new InvalidOperationException("ActiveMap is null");
            ActiveMap.CurrentValue.HandleItemDrop(inventoryIndex);
        }
    }
}