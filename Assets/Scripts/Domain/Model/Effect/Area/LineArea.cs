using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Area
{
    public class LineArea : IDirectionalArea
    {
        public bool ContainsSelf;
        public bool CanIgnoreWalls;
        [MinValue(1)] public int Length;

        public LineArea(int length, bool containsSelf, bool canIgnoreWalls)
        {
            Length = length;
            CanIgnoreWalls = canIgnoreWalls;
            ContainsSelf = containsSelf;
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction, IMap map)
        {
            var area = (ContainsSelf ? Enumerable.Range(0, Length + 1) : Enumerable.Range(1, Length))
                .Select(i => position + (direction.Vector() * i));
            foreach (var pos in area)
            {
                if (!CanIgnoreWalls && !map.IsPassable(pos))
                    break;
                yield return pos;
            }
        }

        public float EvaluateArea()
        {
            return Length;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() =>
            new()
            {
                { new UpgradePath("長さ"), new UpgradeData("長さ+1", () => Length += 1) }
            };

        public string Info()
        {
            var info = $"直線 長さ{Length}マス";
            if (ContainsSelf) info += "(原点含む)";
            if (CanIgnoreWalls) info += "(壁無視)";
            return info;
        }
    }
}