using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class DigEffect : IActorlessEffect
    {
        public Color Color => Colors.Brown;
        public Impact Impact => Impact.Neutral;

        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            return Apply(positions, map);
        }

        public UniTask Apply(IEnumerable<Vector2Int> positions, IMap map)
        {
            map.RemoveWalls(positions);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0;
        }

        public float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions)
        {
            return 0;
        }

        public float EvaluatePrice()
        {
            return 15f;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>();
        }

        public string Info()
        {
            return "壁堀り";
        }
    }
}