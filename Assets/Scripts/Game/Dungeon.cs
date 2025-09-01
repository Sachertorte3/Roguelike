#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Dungeon;
using Domain.Model.Map;
using Domain.Model.Memento;
using UnityEngine;
using Utilities;

namespace Game
{
    public class Dungeon : ISerializable<DungeonMemento>
    {
        private readonly IDungeonData _dungeonData;
        public Id<IMap> StartMapId => _dungeonData.GetStartMapId();
        private readonly Dictionary<int, Id<IMap>> _mapIds;


        public Dungeon(DungeonMemento memento)
        {
            _dungeonData = ScriptableObjectLoader.Load<DungeonBluePrintData>("Dungeon");
            _mapIds = memento.MapIds.ToDictionary(mapId => mapId.Key, mapId => new Id<IMap>(mapId.Value));
        }

        public DungeonMemento Serialize()
        {
            return new DungeonMemento
            (
                new Dictionary<int, string>(_mapIds.ToDictionary(mapIds => mapIds.Key,
                    mapIds => mapIds.Value.ToString()))
            );
        }

        public static DungeonMemento Build()
        {
            return new DungeonMemento
            (
                new Dictionary<int, string>()
            );
        }

        public List<MapConnection> GetDestinations(Id<IMap> mapId)
        {
            return _dungeonData.GetDestinations(mapId);
        }

        public DungeonMapData CreateMapData(Id<IMap> mapId)
        {
            return _dungeonData.CreateMapData(mapId);
        }

        public MapMemento CreateMapManager(Id<IMap> id, IEnumerable<MovementData> movementData)
        {
            var dungeonData = CreateMapData(id);
            if (dungeonData.Field == null)
            {
                var mapBuilder = new WorldMapBuilder(id, "seed");
                foreach (var data in movementData)
                    mapBuilder.AddMovementEntity(data);
                return mapBuilder.Build();
            }
            else
            {
                var mapBuilder = new MapBuilder(dungeonData.Field, dungeonData.WaterChance, dungeonData, id);
                foreach (var data in movementData)
                    mapBuilder.AddMovementEntity(data);
                return mapBuilder.Build();
            }
        }
    }
}