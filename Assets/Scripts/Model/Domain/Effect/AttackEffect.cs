using System;
using Cysharp.Threading.Tasks;
using Data.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Model.Domain.Effect
{
    [Serializable]
    public class AttackEffect : IEffect
    {
        [MinValue(1)] public int Power;
        public Color Color => Colors.Red;

        public AttackEffect(int power)
        {
            Power = power;
        }

        public Impact Impact => Impact.Harmful;

        public async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            await target.LoseHp(Formula.Calc(actor, Power));
            foreach (var ((condition, removalCondition), probability) in actor.AdditionalConditions)
            {
                if (Random.value < probability)
                {
                    target.AddCondition(condition, removalCondition);
                }
            }
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Mathf.Min(1, Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, Power)) / target.CurrentMaxHp);
        }

        public string Info()
        {
            return $"攻撃\n威力: {Power}";
        }
    }
}