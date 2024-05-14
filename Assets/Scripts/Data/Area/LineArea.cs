using Scripts.Utilities;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Data.Area
{
    public class LineArea : IDirectionalArea
    {
        [MinValue(1)] public int Length;
        public bool ContainsSelf;
        public LineArea(int length, bool containsSelf)
        {
            Length = length;
            ContainsSelf = containsSelf;
        }
        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction)
        {
            return (ContainsSelf?
                Enumerable.Range(0, Length+1):
                Enumerable.Range(1, Length))
                .Select(i => position + direction.Vector() * i);
        }
        public string Info() => $"直線　長さ{Length}マス{(ContainsSelf ? "(原点含む)" : "")}";
    }
}