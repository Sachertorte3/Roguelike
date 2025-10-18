using System.Linq;
using Domain.Model.Map;
using UnityEngine;
using Utilities;
using XNode;

namespace Domain.Model.Dungeon
{
    [CreateAssetMenu]
    public class MapGraph : NodeGraph
    {
        public Id<IMap> GetStartMapId()
        {
            return nodes.OfType<MapNode>().Where(node => node.IsStartMapId(node.FirstMapId)).First().FirstMapId;
        }
        public int GetMaxDepth()
        {
            return nodes.OfType<MapNode>().Max(node => node.Depth(node.LastMapId));
        }
        public MapNode GetMapNode(Id<IMap> mapId)
        {
            return nodes.OfType<MapNode>().First(node => node.ContainsMapId(mapId));
        }
    }
}