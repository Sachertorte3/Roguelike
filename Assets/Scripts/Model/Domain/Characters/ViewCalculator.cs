using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

namespace Model.Domain.Characters
{
    internal static class ViewCalculator
    {
        private static readonly OctantTransform[] s_octantTransform =
        {
            new(1, 0, 0, 1), // 0 E-NE
            new(0, 1, 1, 0), // 1 NE-N
            new(0, -1, 1, 0), // 2 N-NW
            new(-1, 0, 0, 1), // 3 NW-W
            new(-1, 0, 0, -1), // 4 W-SW
            new(0, -1, -1, 0), // 5 SW-S
            new(0, 1, -1, 0), // 6 S-SE
            new(1, 0, 0, -1) // 7 SE-E
        };

        public static HashSet<Vector2Int> ComputeCircle(HashSet<Vector2Int> passables, Vector2Int position,
            float radius)
        {
            var viewRadiusSq = radius * radius;
            return ComputeSquare(passables, position, radius).Where(x => (x - position).sqrMagnitude <= viewRadiusSq)
                .ToHashSet();
        }

        public static HashSet<Vector2Int> ComputeSquare(HashSet<Vector2Int> passables, Vector2Int position,
            float radius)
        {
            HashSet<Vector2Int> viewArea = new() { position };
            for (var txidx = 0; txidx < s_octantTransform.Length; txidx++)
                viewArea.AddRange(CastLight(passables, position, radius, 1, 1.0f, 0.0f, s_octantTransform[txidx]));

            return viewArea;
        }

        private static HashSet<Vector2Int> CastLight(HashSet<Vector2Int> passables, Vector2Int origin, float viewRadius,
            int startColumn, float leftViewSlope, float rightViewSlope, OctantTransform txfrm)
        {
            HashSet<Vector2Int> viewArea = new();

            var viewCeiling = (int)Math.Ceiling(viewRadius);
            var prevWasBlocked = false;
            float savedRightSlope = -1;

            for (var currentCol = startColumn; currentCol <= viewCeiling; currentCol++)
            {
                var xc = currentCol;
                for (var yc = currentCol; yc >= 0; yc--)
                {
                    Vector2Int pos = new(origin.x + (xc * txfrm.xx) + (yc * txfrm.xy),
                        origin.y + (xc * txfrm.yx) + (yc * txfrm.yy));

                    var leftBlockSlope = (yc + 0.5f) / (xc - 0.5f);
                    var rightBlockSlope = (yc - 0.5f) / (xc + 0.5f);
                    if (rightBlockSlope > leftViewSlope)
                        continue;
                    if (leftBlockSlope < rightViewSlope) break;

                    var curBlocked = !passables.Contains(pos);
                    viewArea.Add(pos);

                    if (prevWasBlocked)
                    {
                        if (curBlocked)
                        {
                            savedRightSlope = rightBlockSlope;
                        }
                        else
                        {
                            prevWasBlocked = false;
                            leftViewSlope = savedRightSlope;
                        }
                    }
                    else
                    {
                        if (curBlocked)
                        {
                            prevWasBlocked = true;
                            savedRightSlope = rightBlockSlope;
                        }
                    }
                }

                if (prevWasBlocked) break;
            }

            return viewArea;
        }

        private class OctantTransform
        {
            public OctantTransform(int xx, int xy, int yx, int yy)
            {
                this.xx = xx;
                this.xy = xy;
                this.yx = yx;
                this.yy = yy;
            }

            public int xx { get; }
            public int xy { get; }
            public int yx { get; }
            public int yy { get; }
        }
    }
}