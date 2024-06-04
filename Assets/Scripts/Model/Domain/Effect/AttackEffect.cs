using System;
using Cysharp.Threading.Tasks;
using Data.Effect;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Model.Domain.Effect
{
    [Serializable]
    public class AttackEffect : IEffect
    {
        [MinValue(1)] public int Power;

        public AttackEffect(int power)
        {
            Power = power;
        }

        public Impact Impact => Impact.Harmful;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target)
        {
            await target.LoseHp(Formula.Calc(actor, Power));
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1, Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, Power)) / target.MaxHp);
        }

        public string Info()
        {
            return $"攻撃\n威力: {Power}";
        }
    }
}