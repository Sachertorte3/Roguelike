#nullable enable
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using Data.Condition;
using System;


#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/WeaponPrefix")]
    public class WeaponPrefix : ScriptableObject
    {
        [ReadOnly, Required] public string Name;
        [MinValue(0)] public float PowerMagnification = 1;
        [MinValue(0)] public float UsageLimitMagnification = 1;
        public List<AdditionalConditionData> AdditionalConditions = new();
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.SaveAssets();
        }
#endif
    }
    [Serializable]
    public class AdditionalConditionData : IHasInfo
    {
        [Required, SerializeReference] public IConditionData Condition;
        [Required] public RemovalConditionData RemovalCondition;
        [Range(0, 1)] public float Probability;
        public AdditionalConditionData(IConditionData condition, RemovalConditionData removalCondition, float probability)
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

