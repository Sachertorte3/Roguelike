#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Memento;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Model.Game
{
    public class Dungeon : ISerializable<DungeonMemento>
    {
        private readonly DungeonBluePrintData _dungeonData;
        private readonly Dictionary<int, Id<MapManager>> _mapIds;
        public Dungeon(DungeonMemento memento)
        {
            _dungeonData = Addressables.LoadAssetAsync<DungeonBluePrintData>($"Assets/Database/DungeonBluePrintData/{memento.DungeonDataName}.asset").WaitForCompletion();
            _mapIds = memento.MapIds.ToDictionary(mapId => mapId.Key, mapId => new Id<MapManager>(mapId.Value));
        }
        public DungeonMemento Serialize()
        {
            return new DungeonMemento
            {
                DungeonDataName = _dungeonData.name,
                MapIds = new(_mapIds.ToDictionary(mapIds => mapIds.Key, mapIds => mapIds.Value.Value)),
            };
        }
        public static DungeonMemento Build(DungeonBluePrintData _dungeonData)
        {
            return new DungeonMemento
            {
                DungeonDataName = _dungeonData.name,
                MapIds = new(),
            };
        }
        public Id<MapManager> GetMapId(int level)
        {
            if (!_mapIds.ContainsKey(level))
            {
                var mapId = UniqueIdGenerator.Generate<MapManager>();
                _mapIds[level] = mapId;
            }
            return _mapIds[level];
        }
        public DungeonMapData CreateMapData(int level) => _dungeonData.CreateMapData(level);
    }
}