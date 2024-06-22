using System;
using Cysharp.Threading.Tasks;
using Data.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Model.Domain.Effect
{
    [Serializable]
    public class HealEffect : IEffect
    {
        [MinValue(1)] public int Power;

        public HealEffect(int power)
        {
            Power = power;
        }

        public Color Color => Colors.Green;

        public Impact Impact => Impact.Beneficial;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            await target.GainHp(Formula.Calc(actor, Power));
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1,
                Mathf.Min(target.CurrentMaxHp - target.CurrentHp, (float)Formula.Calc(actor, Power)) /
                target.CurrentMaxHp);
        }

        public string Info()
        {
            return $"回復\n威力: {Power}";
        }
    }
}