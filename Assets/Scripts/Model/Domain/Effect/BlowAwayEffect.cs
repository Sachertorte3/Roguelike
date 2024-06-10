using System;
using Cysharp.Threading.Tasks;
using Data.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Model.Domain.Effect
{
    [Serializable]
    public class BlowAwayEffect : IEffect
    {
        [MinValue(1)] public int Distance;
        public Color Color => Colors.LightGreen;

        public BlowAwayEffect(int distance)
        {
            Distance = distance;
        }

        public Impact Impact => Impact.Harmful;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            await target.BlowAway(DirectionMethods.NearestDirectionFromVector(target.CurrentPosition - actor.CurrentPosition).Value, Distance, map);
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0;
        }

        public string Info()
        {
            return "吹き飛ばし";
        }
    }
}