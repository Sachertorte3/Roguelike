#nullable enable
using System;
using Domain.Model.Condition;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Model.Character
{
    [Serializable]
    public class AdditionalConditionData : IHasInfo
    {
        [Required] public ScriptableObjectSerializable<ConditionTemplate> Condition;
        [Range(0, 1)] public float Probability;

        public AdditionalConditionData(IConditionData condition, RemovalConditionData removalCondition,
            float probability)
        {
            Condition = new(new ConditionTemplate(condition, removalCondition));
            Probability = probability;
        }

        public string Info()
        {
            return $"{Condition.Value.Condition.Name} {Probability:P0}";
        }
    }
}