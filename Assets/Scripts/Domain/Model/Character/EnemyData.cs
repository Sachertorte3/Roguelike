using Domain.Model.Character.Type;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Domain.Model.Effect;

#if UNITY_EDITOR
using System.IO;
#endif

namespace Domain.Model.Character
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [ReadOnly] [Required] public string Name = "";
        public CharacterGroup Group = CharacterGroup.Enemy;
        public bool IsBoss = false;
        [MinValue(1)] public int Hp;
        public Aggression Aggression = Aggression.AvoidAllies;
        public bool WanderAround = true;
        public SkillData[] Skills;
        public List<AdditionalConditionData> AdditionalConditions = new();
        [SerializeReference] public ICharacterType CharacterType;
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}