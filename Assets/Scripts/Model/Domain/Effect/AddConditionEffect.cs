using System;
using Cysharp.Threading.Tasks;
using Data.Condition;
using Data.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Model.Domain.Effect
{
    [Serializable]
    public class AddConditionEffect : IEffect
    {
        [SerializeReference][Required] public IConditionData Condition;
        [Required] public RemovalConditionData RemovalCondition;
        public Color Color => Colors.Purple;

        public AddConditionEffect(IConditionData condition, RemovalConditionData removalCondition)
        {
            Condition = condition;
            RemovalCondition = removalCondition;
        }

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