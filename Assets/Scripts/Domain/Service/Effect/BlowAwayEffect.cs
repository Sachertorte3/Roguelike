using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
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

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            await target.BlowAway(
                DirectionMethods.NearestDirectionFromVector(target.CurrentPosition - actor.CurrentPosition).Value,
                _distance, map);
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0;
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