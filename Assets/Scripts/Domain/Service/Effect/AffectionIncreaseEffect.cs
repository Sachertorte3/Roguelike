using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AffectionIncreaseEffect : IEffect
    {
        [MinValue(1)] public float Power;

        public AffectionIncreaseEffect(float power)
        {
            Power = power;
        }

        public Color Color => Colors.HotPink;

        public Impact Impact => Impact.Beneficial;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Power;
        }

        public string Info()
        {
            return $"好感度上昇\n威力: {Power}";
        }
    }
}