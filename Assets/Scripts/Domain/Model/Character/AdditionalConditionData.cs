#nullable enable
using Sirenix.OdinInspector;
using UnityEngine;
using Domain.Model.Condition;
using System;

namespace Domain.Model.Character
{
    [Serializable]
    public class AdditionalConditionData : IHasInfo
    {
        [Required] public ConditionTemplate Condition;
        [Range(0, 1)] public float Probability;

        public AdditionalConditionData(IConditionData condition, RemovalConditionData removalCondition,
            float probability)
        {
            Condition = new ConditionTemplate(condition, removalCondition);
            Probability = probability;
        }

        public string Info()
        {
            return $"{Condition.Condition.Name} {Probability:P0}";
        }
    }
}