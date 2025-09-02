#nullable enable
using System.Collections.Generic;
using Domain.Model.Map;
using Utilities;

namespace Domain.Model.Dungeon
{
    public interface IDungeonData
    {
        public string Name { get; }
        public Id<IMap> GetStartMapId();
        public List<MapConnection> GetDestinations(Id<IMap> mapId);
        public DungeonMapData CreateMapData(Id<IMap> mapId);
    }
}