#nullable enable
using Domain.Model.Condition;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Domain.Model.Character
{
    [CreateAssetMenu(fileName = "Condition", menuName = "ScriptableObject/ConditionTemplate")]
    public class ConditionTemplate : ScriptableObject
    {
        [Required, SerializeReference] public IConditionData Condition;
        [Required] public RemovalConditionData RemovalCondition;
        public ConditionTemplate(IConditionData condition, RemovalConditionData removalCondition)
        {
            Condition = condition;
            RemovalCondition = removalCondition;
        }
        public float Evaluate(ITargetOfEffect target) => Condition.Evaluate(target) * RemovalCondition.EvaluateTurn();
        public float EvaluateDamage() => Condition.EvaluateDamage() * RemovalCondition.EvaluateTurn();
    }
}