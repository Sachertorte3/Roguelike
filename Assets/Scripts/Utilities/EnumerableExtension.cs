using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Utilities
{
    public static class EnumerableExtension
    {
        public static IEnumerable<Vector2Int> RectRange(this RectInt rect)
        {
            for (var x = rect.x; x < rect.x + rect.width; x++)
            {
                for (var y = rect.y; y < rect.y + rect.height; y++)
                {
                    yield return new Vector2Int(x, y);
                }
            }
        }
    }
}
