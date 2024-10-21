using System.Collections.Generic;
using System.Linq;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Area
{
    public class CircleArea : INotDirectionalArea
    {
        public bool ContainsSelf;
        public bool CanIgnoreWalls;
        [MinValue(1)] public int Radius;

        public CircleArea(int radius, bool containsSelf, bool canIgnoreWalls)
        {
            Radius = radius;
            ContainsSelf = containsSelf;
            CanIgnoreWalls = canIgnoreWalls;
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, IMap map)
        {
            if (CanIgnoreWalls || Radius <= 1)
                return EnumerableExtension.CircleRange(position, Radius + 0.5f)
                    .Where(p => ContainsSelf || p != position);
            return ViewCalculator.ComputeCircle(map.GetAllBlankPositionsOn(EntityLayer.Middle), position, Radius + 0.5f)
                .Where(p => ContainsSelf || p != position);
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction, IMap map)
        {
            return Get(position, map);
        }

        public float EvaluateArea()
        {
            return CommonSenseParameters.CircleAreaEvaluate(CanIgnoreWalls, Radius);
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>
            {
                {
                    new UpgradePath("半径"),
                    new UpgradeData(
                        "半径+1",
                        () => Radius += 1,
                        () => Radius -= 1
                    )
                }
            };
        }

        public string Info()
        {
            var info = $"円 半径{Radius}マス";
            if (ContainsSelf) info += "(原点含む)";
            if (CanIgnoreWalls) info += "(壁無視)";
            return info;
        }
    }
}