using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class TeleportToAreaEffect : IEffect
    {
        public Impact Impact => Impact.Neutral;
        public Color Color => Colors.SkyBlue;

        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            var position = positions.GetAtRandom();
            var blank = map.FindBlankPositionFrom(position,
                (pos) => actor.IsFlying ? map.IsBlank(pos, EntityLayer.Middle) : map.IsBlankAndStandable(pos, EntityLayer.Middle));
            actor.Teleport(blank);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.1f;
        }

        public float EvaluatePrice()
        {
            return 50f;
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades()
        {
            return new Dictionary<UpgradePath, UpgradeData>();
        }

        public string Info()
        {
            return "テレポート";
        }
    }
}