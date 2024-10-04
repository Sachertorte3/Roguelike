using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities.Algorithms
{
    public class BlankFinder
    {
        private static readonly List<Vector2Int> Directions = new()
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.up + Vector2Int.right,
            Vector2Int.down + Vector2Int.right,
            Vector2Int.up + Vector2Int.left,
            Vector2Int.down + Vector2Int.left
        };

        public static Vector2Int FindBlankPosition(Func<Vector2Int, bool> isBlankFunc,
            Func<Vector2Int, bool> isFloorFunc, Vector2Int position) //FIXME Error when search results are not found
        {
            var openedPos = new List<Vector2Int>() { position };
            var nextPos = new List<Vector2Int>();
            var closedPos = new List<Vector2Int>();
            while (openedPos.Count < 2000)
            {
                closedPos.AddRange(openedPos);
                foreach (var pos in openedPos)
                {
                    if (isFloorFunc(pos) && isBlankFunc(pos))
                    {
                        return pos;
                    }

                    nextPos.AddRange(Directions
                        .Select(x => pos + x)
                        .Where(x => !closedPos.Contains(x) && !nextPos.Contains(x)));
                }

                openedPos = nextPos;
                nextPos = new List<Vector2Int>();
            }

            return position;
        }
    }
}