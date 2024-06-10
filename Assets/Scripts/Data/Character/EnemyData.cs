using Data.Character.Type;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Data.Condition;
using System;




#if UNITY_EDITOR
using System.IO;
#endif

namespace Data.Character
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [ReadOnly, Required] public string Name = "";
        [MinValue(1)] public int Hp;
        [SerializeReference] public ICharacterType CharacterType;
        public Aggression Aggression = Aggression.AvoidAllies;
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
    public class AdditionalConditionData
    {
        [Required, SerializeReference] public IConditionData Condition;
        [Required] public RemovalConditionData RemovalCondition;
        [Range(0, 1)] public float Probability;
    }
}

