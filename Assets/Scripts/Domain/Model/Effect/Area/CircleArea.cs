using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Model.Area
{
    public class CircleArea : INotDirectionalArea
    {
        public bool ContainsSelf;
        [MinValue(1)] public int Radius;

        public CircleArea(int radius, bool containsSelf)
        {
            Radius = radius;
            ContainsSelf = containsSelf;
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position)
        {
            return EnumerableExtension.CircleRange(position, Radius + 0.5f).Where(p => ContainsSelf || p != position);
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction)
        {
            return Get(position);
        }

        public string Info()
        {
            return $"円　半径{Radius}マス{(ContainsSelf ? "(原点含む)" : "")}";
        }
    }
}