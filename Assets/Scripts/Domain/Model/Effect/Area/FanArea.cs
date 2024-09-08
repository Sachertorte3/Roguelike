using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Area
{
    public class FanArea : IDirectionalArea
    {
        public bool ContainsSelf;
        public bool CanIgnoreWalls;
        [MinValue(1)] public int Radius;

        public FanArea(int radius, bool containsSelf, bool canIgnoreWalls)
        {
            Radius = radius;
            ContainsSelf = containsSelf;
            CanIgnoreWalls = canIgnoreWalls;
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction, IMap map)
        {
            var area = new List<Vector2Int>();

            switch (direction)
            {
                case Direction8.Up:
                case Direction8.Down:
                case Direction8.Left:
                case Direction8.Right:
                    var deltaVec = direction.Vector();
                    var perpVec = direction.Rotate90Clockwise().Vector();
                    for (var i = 1; i <= Radius; i++)
                    {
                        for (var j = -i; j <= i; j++)
                        {
                            if ((i * i) + (j * j) <= (Radius + 0.5f) * (Radius + 0.5f))
                            {
                                area.Add(position + (i * deltaVec) + (j * perpVec));
                            }
                        }
                    }

                    break;

                case Direction8.UpLeft:
                case Direction8.UpRight:
                case Direction8.DownLeft:
                case Direction8.DownRight:
                    var clockwiseVec = direction.Rotate45Clockwise().Vector();
                    var anticlockwiseVec = direction.Rotate45AntiClockwise().Vector();
                    for (var i = 1; i <= Radius; i++)
                    {
                        area.Add(position + (i * clockwiseVec));
                        for (var j = 1; j <= Radius; j++)
                        {
                            area.Add(position + (j * anticlockwiseVec));
                            if ((i * i) + (j * j) <= (Radius + 0.5f) * (Radius + 0.5f))
                            {
                                area.Add(position + (i * clockwiseVec) + (j * anticlockwiseVec));
                            }
                        }
                    }

                    break;
            }

            if (CanIgnoreWalls)
                return area;
            var reachable = ViewCalculator.ComputeSquare(map.GetAllPassablePositions(), position, Radius + 0.5f);
            return area.Where(p => reachable.Contains(p));
        }

        public float EvaluateArea()
        {
            if (CanIgnoreWalls)
                return (Radius + 1) * 1.5f;
            return Radius + 1;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() =>
            new()
            {
                { new UpgradePath("半径"), new UpgradeData("半径+1", () => Radius += 1) }
            };

        public string Info()
        {
            var info = $"扇形(90°) 半径{Radius}マス";
            if (ContainsSelf) info += "(原点含む)";
            if (CanIgnoreWalls) info += "(壁無視)";
            return info;
        }
    }
}