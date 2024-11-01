using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class TeleportToAreaEffect : FieldTargetEffect
    {
        public override Impact Impact => Impact.Neutral;
        public override Color Color => Colors.SkyBlue;

        public override UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            var placeablePositions = positions.Where(pos => map.At(pos).CanPlace(actor.IsFlying, actor.CanThroughWalls, false, EntityLayer.Middle));
            if (placeablePositions.Any())
            {
                actor.Teleport(placeablePositions.GetAtRandom());
            }
            else
            {
                actor.Teleport(map.FindBlankPositionFrom(positions.GetAtRandom(), pos => map.At(pos).CanPlace(actor.IsFlying, actor.CanThroughWalls, false, EntityLayer.Middle)));
            }
            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions)
        {
            if (positions.Contains(actor.CurrentPosition))
                return 0;
            return 0.05f * positions.Average(pos => VectorExtension.ChebyshevDistance(actor.CurrentPosition, pos));
        }

        public override float EvaluatePrice()
        {
            return 50f;
        }

        public override string UpgradePathName => "テレポート";
        public override List<UpgradeData> GetUpgrades() => new();
        public override List<IHasUpgrades> GetChildren() => new();

        public override string Info()
        {
            return "テレポート";
        }
    }
}