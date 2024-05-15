using Cysharp.Threading.Tasks;
using Data;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Model.Effect
{
    [Serializable]
    public class AttackEffect : IEffect
    {
        [MinValue(1)] public int Power;

        public AttackEffect(int power)
        {
            Power = power;
        }
        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target)
        {
            await target.LoseHp(Formula.Calc(actor, Power));
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1, (float)Formula.Calc(actor, Power) / target.MaxHp);
        }

        public string Info()
        {
            return $"攻撃\n威力: {Power}";
        }
    }
}