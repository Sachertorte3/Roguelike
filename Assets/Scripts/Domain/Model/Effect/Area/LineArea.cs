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
        [MinValue(1)] public int Length;

        public LineArea(int length, bool containsSelf)
        {
            Length = length;
            ContainsSelf = containsSelf;
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction)
        {
            return (ContainsSelf ? Enumerable.Range(0, Length + 1) : Enumerable.Range(1, Length))
                .Select(i => position + direction.Vector() * i);
        }

        public Dictionary<UpgradePath, System.Action> _GetUpgrades() =>
            new Dictionary<UpgradePath, System.Action> {
                { new UpgradePath("Length"), () => Length += 1 }
            };

        public string Info()
        {
            return $"直線 長さ{Length}マス{(ContainsSelf ? "(原点含む)" : "")}";
        }
    }
}