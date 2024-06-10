using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Data.Area
{
    public class FanArea : IDirectionalArea
    {
        public bool ContainsSelf;
        [MinValue(1)] public int Radius;

        public FanArea(int radius, bool containsSelf)
        {
            Radius = radius;
            ContainsSelf = containsSelf;
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction)
        {
            List<Vector2Int> area = new List<Vector2Int>();

            switch (direction)
            {
                case Direction8.Up:
                case Direction8.Down:
                case Direction8.Left:
                case Direction8.Right:
                    Vector2Int deltaVec = direction.Vector();
                    Vector2Int perpVec = direction.Rotate90Clockwise().Vector();
                    for (int i = 1; i <= Radius; i++)
                    {
                        for (int j = -i; j <= i; j++)
                        {
                            if (i * i + j * j <= (Radius + 0.5f) * (Radius + 0.5f))
                            {
                                area.Add(position + i * deltaVec + j * perpVec);
                            }
                        }
                    }
                    break;

                case Direction8.UpLeft:
                case Direction8.UpRight:
                case Direction8.DownLeft:
                case Direction8.DownRight:
                    Vector2Int clockwiseVec = direction.Rotate45Clockwise().Vector();
                    Vector2Int anticlockwiseVec = direction.Rotate45AntiClockwise().Vector();
                    for (int i = 1; i <= Radius; i++)
                    {
                        area.Add(position + i * clockwiseVec);
                        for (int j = 1; j <= Radius; j++)
                        {
                            area.Add(position + j * anticlockwiseVec);
                            if (i * i + j * j <= (Radius + 0.5f) * (Radius + 0.5f))
                            {
                                area.Add(position + i * clockwiseVec + j * anticlockwiseVec);
                            }
                        }
                    }
                    break;
            }

            return area;
        }

        public string Info()
        {
            return $"扇形(90°) 半径{Radius}マス{(ContainsSelf ? "(原点含む)" : "")}";
        }
    }
}