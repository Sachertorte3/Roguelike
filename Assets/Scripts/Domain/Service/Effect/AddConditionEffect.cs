using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AddConditionEffect : IEffect
    {
        [Required] public RemovalConditionData RemovalCondition;
        [SerializeReference][Required] public IConditionData Condition;

        public AddConditionEffect(IConditionData condition, RemovalConditionData removalCondition)
        {
            Condition = condition;
            RemovalCondition = removalCondition;
        }

        public Color Color => Colors.Purple;

        public Impact Impact => Condition.Impact;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, IPassableChecker map)
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