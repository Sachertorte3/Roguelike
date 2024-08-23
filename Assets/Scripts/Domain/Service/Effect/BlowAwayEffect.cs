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

        public IEnumerable<UpgradeSkill> GenerateUpgrades()
        {
            return new List<UpgradeSkill>
            {
                new UpgradeSkill(
                    () => _distance += 1,
                    1
                )
            };
        }

        public string Info()
        {
            return $"吹き飛ばし{_distance}マス";
        }
    }
}