using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class BlowAwayEffect : EntityTargetEffect
    {
        [MinValue(1)][SerializeField] private int _distance;

        public BlowAwayEffect(int distance)
        {
            _distance = distance;
        }

        public override Color Color => Colors.LightGreen;

        public override Impact Impact => Impact.Harmful;

        public override async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map)
        {
            if (!target.StatusManager.IsHeavy)
            {
                await Apply(actor, (IEntity)target, position, map);
            }
        }

        public override async UniTask Apply(IActorOfEffect actor, IEntity target, Vector2Int position, IMap map)
        {
            var direction = DirectionMethods.NearestDirectionFromVector(target.CurrentPosition - actor.CurrentPosition);
            if (direction.HasValue)
                await target.BlowAway(actor, direction.Value, _distance, map);
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            if (target.StatusManager.IsHeavy)
            {
                return 0f;
            }
            return CommonSenseParameters.BlowAwayEvaluate(_distance);
        }

        public override float EvaluatePrice()
        {
            return CommonSenseParameters.BlowAwayPrice(_distance);
        }

        public override string UpgradePathName => "吹き飛ばし";
        public override List<UpgradeData> GetUpgrades()
        {
            return new List<UpgradeData>
            {
                new UpgradeData(
                    "吹き飛ばし距離+1",
                    () => _distance += 1,
                    () => _distance -= 1
                )
            };
        }

        public override Dictionary<string, IHasUpgrades> GetChildren() => new();

        public override string Info()
        {
            return $"吹き飛ばし{_distance}マス";
        }
    }
}