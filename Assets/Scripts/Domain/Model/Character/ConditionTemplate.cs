#nullable enable
using Sirenix.OdinInspector;
using UnityEngine;
using Domain.Model.Condition;

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
    }
}