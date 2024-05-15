using Cysharp.Threading.Tasks;
using Data;
using Data.Condition;
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
    [Serializable]
    public class AddConditionEffect : IEffect
    {
        [SerializeReference, Required] public IConditionData Condition;
        [Required] public RemovalConditionData RemovalCondition;

        public AddConditionEffect(IConditionData condition, RemovalConditionData removalCondition)
        {
            Condition = condition;
            RemovalCondition = removalCondition;
        }
        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target)
        {
            target.AddCondition(Condition, RemovalCondition);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 1;
        }

        public string Info()
        {
            return $"状態付与: {Condition.Name}";
        }
    }
}