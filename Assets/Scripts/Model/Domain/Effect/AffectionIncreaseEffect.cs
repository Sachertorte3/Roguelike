using System;
using Cysharp.Threading.Tasks;
using Data.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Model.Domain.Effect
{
    [Serializable]
    public class AffectionIncreaseEffect : IEffect
    {
        [MinValue(1)] public float Power;
        public Color Color => Colors.Pink;

        public AffectionIncreaseEffect(float power)
        {
            Power = power;
        }

        public Impact Impact => Impact.Beneficial;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {

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