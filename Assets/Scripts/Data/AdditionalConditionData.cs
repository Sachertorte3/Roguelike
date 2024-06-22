#nullable enable
using Sirenix.OdinInspector;
using UnityEngine;
using Data.Condition;
using System;


#if UNITY_EDITOR
#endif

namespace Data
{
    [Serializable]
    public class AdditionalConditionData : IHasInfo
    {
        [Required] public RemovalConditionData RemovalCondition;
        [Range(0, 1)] public float Probability;
        [Required][SerializeReference] public IConditionData Condition;

        public AdditionalConditionData(IConditionData condition, RemovalConditionData removalCondition,
            float probability)
        {
            Condition = condition;
            RemovalCondition = removalCondition;
            Probability = probability;
        }

        public string Info()
        {
            return $"{Condition.Name} {Probability:%}";
        }
    }
}