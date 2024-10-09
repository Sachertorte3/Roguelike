using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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
            var placeablePositions = positions.Where(pos => map.CanPlace(pos, actor.IsFlying));
            if (placeablePositions.Any())
            {
                actor.Teleport(placeablePositions.GetAtRandom());
            }
            else
            {
                actor.Teleport(map.FindBlankPositionFrom(positions.GetAtRandom(), (pos) => map.CanPlace(pos, actor.IsFlying)));
            }
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0;
        }

        public float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions)
        {
            if (positions.Contains(actor.CurrentPosition))
                return 0;
            return 0.05f * positions.Average(pos => VectorExtension.ChebyshevDistance(actor.CurrentPosition, pos));
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