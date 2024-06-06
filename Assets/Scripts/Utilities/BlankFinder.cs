using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities.Algorithms
{
    public class BlankFinder
    {
        public static Vector2Int FindBlankPosition(Func<Vector2Int, bool> isBlankFunc,
            Func<Vector2Int, bool> isFloorFunc, Vector2Int position) //FIXME Error when search results are not found
        {
            var openedPos = new List<Vector2Int>() { position };
            var nextPos = new List<Vector2Int>();
            var closedPos = new List<Vector2Int>();
            while (openedPos.Count < 20)
            {
                closedPos.AddRange(openedPos);
                foreach (var pos in openedPos)
                {
                    if (isFloorFunc(pos) && isBlankFunc(pos))
                    {
                        return pos;
                    }

                    nextPos.AddRange(DirectionMethods.AllDirections.Select(x => pos + x.Vector())
                        .Where(x => !closedPos.Contains(x) && !nextPos.Contains(x)).Where(x => isFloorFunc(x)));
                }

                openedPos = nextPos;
                nextPos = new List<Vector2Int>();
            }

            return position;
        }
    }
}