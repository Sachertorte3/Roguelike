#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Dungeon;
using Domain.Model.Map;
using Domain.Model.Memento;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Game
{
    public class Dungeon : ISerializable<DungeonMemento>
    {
        private readonly DungeonBluePrintData _dungeonData;
        private readonly Dictionary<int, Id<IMap>> _mapIds;
        private readonly ItemDatabase _itemDatabase;

        public Dungeon(DungeonMemento memento)
        {
            _dungeonData = Addressables
                .LoadAssetAsync<DungeonBluePrintData>(
                    $"Assets/Database/DungeonBluePrintData/{memento.DungeonDataName}.asset").WaitForCompletion();
            _mapIds = memento.MapIds.ToDictionary(mapId => mapId.Key, mapId => new Id<IMap>(mapId.Value));
            _itemDatabase = new ItemDatabase(memento.ItemTable, _dungeonData.MasterItemDataBase, _dungeonData.SpawnItem);
        }

        public DungeonMemento Serialize()
        {
            return new DungeonMemento
            (
                _dungeonData.name,
                new Dictionary<int, string>(_mapIds.ToDictionary(mapIds => mapIds.Key,
                    mapIds => mapIds.Value.ToString())),
                _itemDatabase.Serialize()
            );
        }

        public static DungeonMemento Build(DungeonBluePrintData _dungeonData)
        {
            return new DungeonMemento
            (
                _dungeonData.name,
                new Dictionary<int, string>(),
                ItemDatabase.Build(_dungeonData.Placeholders)
            );
        }

        public bool ExistLevel(int level)
        {
            return _dungeonData.ExistLevel(level);
        }

        public Id<IMap> GetMapId(int level)
        {
            if (!_mapIds.ContainsKey(level))
            {
                var mapId = Id<IMap>.Generate();
                _mapIds[level] = mapId;
            }

            return _mapIds[level];
        }

        public DungeonMapData CreateMapData(int level)
        {
            return _dungeonData.CreateMapData(level, _itemDatabase);
        }

        public MapMemento CreateMapManager(Id<IMap> id,
        int level, Id<IEntity>? upStairsId, Id<IEntity>? upStairsDestinationId,
            Id<IEntity>? downStairsId, Id<IEntity>? downStairsDestinationId,
            Location? magicCircleLocation, Id<IEntity>? magicCircleId, Id<IEntity>? magicCircleDestinationId,
            Location? magicCirclePrevLocation, Id<IEntity>? magicCirclePrevId, Id<IEntity>? magicCirclePrevDestinationId)
        {
            var dungeonData = CreateMapData(level);
            var mapBuilder = new MapBuilder(dungeonData.Field, dungeonData.WaterChance, dungeonData, new Location(_dungeonData.name, level));
            if (ExistLevel(level + 1))
                mapBuilder.AddDownStairs(dungeonData, new Location(_dungeonData.name, level + 1), downStairsId, downStairsDestinationId);
            if (ExistLevel(level - 1))
                mapBuilder.AddUpStairs(dungeonData, new Location(_dungeonData.name, level - 1), upStairsId, upStairsDestinationId);
            if (magicCircleLocation != null)
                mapBuilder.AddMagicCircle(dungeonData, magicCircleLocation, magicCircleId, magicCircleDestinationId);
            if (magicCirclePrevLocation != null)
                mapBuilder.AddMagicCircle(dungeonData, magicCirclePrevLocation, magicCirclePrevId, magicCirclePrevDestinationId);
            return mapBuilder.Build(id);
        }
    }
}