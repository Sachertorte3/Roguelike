using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AddConditionEffect : IEffect
    {
        [Required, SerializeField] private ConditionTemplate _condition;

        public AddConditionEffect(ConditionTemplate condition)
        {
            _condition = condition;
        }

        public Color Color => Colors.Purple;

        public Impact Impact => _condition.Condition.Impact;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
        {
            target.AddCondition(_condition.Condition, _condition.RemovalCondition);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 1;
        }

        public string Info()
        {
            return $"状態付与: {_condition.Condition.Name}";
        }
    }
}