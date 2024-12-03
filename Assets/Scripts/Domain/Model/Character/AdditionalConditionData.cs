#nullable enable
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Character
{
    [Serializable]
    public class AdditionalConditionData : IHasInfo
    {
        [Required] public ScriptableObjectSerializable<ConditionTemplate> Condition;
        [Range(0, 1)] public float Probability;

        public string Info()
        {
            return $"{Condition.Value.Condition.Name} {Probability:P0}";
        }
    }
}