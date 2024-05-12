using Scripts.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Data.Area
{
    public class LineArea: IDirectionalArea
    {
        public int Length;
        public LineArea(int length)
        {
            Length = length;
        }
        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction)
        {
            return Enumerable.Range(1, Length).Select(i => position + direction.Vector() * i);
        }
    }
}