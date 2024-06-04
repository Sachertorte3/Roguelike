using System;
using Cysharp.Threading.Tasks;
using Data.Effect;
using Sirenix.OdinInspector;
using UnityEngine;

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

        public Impact Impact => Impact.Beneficial;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target)
        {
            await target.GainHp(Formula.Calc(actor, Power));
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1, Mathf.Min(target.MaxHp - target.CurrentHp, Formula.Calc(actor, Power)) / target.MaxHp);
        }

        public string Info()
        {
            return $"回復\n威力: {Power}";
        }
    }
}