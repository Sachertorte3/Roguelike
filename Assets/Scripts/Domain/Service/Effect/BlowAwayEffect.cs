using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class BlowAwayEffect : IEffect
    {
        [MinValue(1), SerializeField] private int _distance;

        public BlowAwayEffect(int distance)
        {
            _distance = distance;
        }

        public Color Color => Colors.LightGreen;

        public Impact Impact => Impact.Harmful;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IMap map) =>
            await Apply(actor, (IEntity)target, map);
        public async UniTask Apply(IActorOfEffect actor, IEntity target, IMap map)
        {
            var direction = DirectionMethods.NearestDirectionFromVector(target.CurrentPosition - actor.CurrentPosition);
            if (direction.HasValue)
                await target.BlowAway(actor, direction.Value, _distance, map);
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return CommonSenseParameters.BlowAwayEvaluate(_distance);
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.BlowAwayPrice(_distance);
        }

        public Dictionary<UpgradePath, UpgradeData> GetUpgrades() => new()
        {
            { new UpgradePath("吹き飛ばし距離"), new UpgradeData("吹き飛ばし距離+1", () => _distance += 1) }
        };

        public string Info()
        {
            return $"吹き飛ばし{_distance}マス";
        }
    }
}