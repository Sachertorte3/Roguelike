using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Model.Characters
{
    public static class ViewCalculator
    {
        private class OctantTransform
        {
            public int xx { get; private set; }
            public int xy { get; private set; }
            public int yx { get; private set; }
            public int yy { get; private set; }

            public OctantTransform(int xx, int xy, int yx, int yy)
            {
                this.xx = xx;
                this.xy = xy;
                this.yx = yx;
                this.yy = yy;
            }
        }

        private static OctantTransform[] s_octantTransform =
        {
            new OctantTransform(1, 0, 0, 1), // 0 E-NE
            new OctantTransform(0, 1, 1, 0), // 1 NE-N
            new OctantTransform(0, -1, 1, 0), // 2 N-NW
            new OctantTransform(-1, 0, 0, 1), // 3 NW-W
            new OctantTransform(-1, 0, 0, -1), // 4 W-SW
            new OctantTransform(0, -1, -1, 0), // 5 SW-S
            new OctantTransform(0, 1, -1, 0), // 6 S-SE
            new OctantTransform(1, 0, 0, -1), // 7 SE-E
        };

        public static HashSet<Vector2Int> ComputeCircle(HashSet<Vector2Int> passables, Vector2Int position, float radius)
        {
            float viewRadiusSq = radius * radius;
            return ComputeSquare(passables, position, radius).Where(x => (x - position).sqrMagnitude <= viewRadiusSq)
                .ToHashSet();
        }

        public static HashSet<Vector2Int> ComputeSquare(HashSet<Vector2Int> passables, Vector2Int position, float radius)
        {
            HashSet<Vector2Int> viewArea = new HashSet<Vector2Int> { position };
            for (int txidx = 0; txidx < s_octantTransform.Length; txidx++)
            {
                viewArea.AddRange(CastLight(passables, position, radius, 1, 1.0f, 0.0f, s_octantTransform[txidx]));
            }

            return viewArea;
        }

        private static HashSet<Vector2Int> CastLight(HashSet<Vector2Int> passables, Vector2Int origin, float viewRadius,
            int startColumn, float leftViewSlope, float rightViewSlope, OctantTransform txfrm)
        {
            HashSet<Vector2Int> viewArea = new HashSet<Vector2Int>();

            int viewCeiling = (int)Math.Ceiling(viewRadius);
            bool prevWasBlocked = false;
            float savedRightSlope = -1;

            for (int currentCol = startColumn; currentCol <= viewCeiling; currentCol++)
            {
                int xc = currentCol;
                for (int yc = currentCol; yc >= 0; yc--)
                {
                    Vector2Int pos = new Vector2Int(origin.x + (xc * txfrm.xx) + (yc * txfrm.xy),
                        origin.y + (xc * txfrm.yx) + (yc * txfrm.yy));

                    float leftBlockSlope = (yc + 0.5f) / (xc - 0.5f);
                    float rightBlockSlope = (yc - 0.5f) / (xc + 0.5f);
                    if (rightBlockSlope > leftViewSlope)
                    {
                        continue;
                    }
                    else if (leftBlockSlope < rightViewSlope)
                    {
                        break;
                    }

                    bool curBlocked = !passables.Contains(pos);
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

                if (prevWasBlocked)
                {
                    break;
                }
            }

            return viewArea;
        }
    }
}