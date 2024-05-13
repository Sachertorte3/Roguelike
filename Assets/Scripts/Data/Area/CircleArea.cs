using Scripts.Utilities;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Data.Area
{
    public class CircleArea : INotDirectionalArea
    {
        [MinValue(1)] public int Radius;
        public bool ContainsSelf;
        public CircleArea(int radius, bool containsSelf)
        {
            Radius = radius;
            ContainsSelf = containsSelf;
        }
        public IEnumerable<Vector2Int> Get(Vector2Int position)
        {
            return EnumerableExtension.CircleRange(position, Radius+0.5f).Where(p => ContainsSelf || p != position);
        }
        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction) => Get(position);
    }
}