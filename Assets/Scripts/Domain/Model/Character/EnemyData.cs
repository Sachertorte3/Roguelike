#nullable enable
using Domain.Model.Character.Type;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Domain.Model.Effect;
using System;
using Domain.Model.Item;

#if UNITY_EDITOR
using System.IO;
#endif

namespace Domain.Model.Character
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [ReadOnly][Required] public string Name = "";
        public CharacterGroup Group = CharacterGroup.Monster;
        [SerializeReference] public ICharacterType CharacterType;
        public bool IsBoss = false;
        [MinValue(1)] public int Hp;
        public Aggression Aggression = Aggression.AvoidAllies;
        public BehaviorData Behavior;
        public MoveSpeed MoveSpeed = MoveSpeed.Normal;
        public bool CanPickUp = false;
        public bool CanUseItem = false;
        public EnemySkillData[] Skills;
        public bool HasLastSkill = false;
        [ShowIf("@HasLastSkill")] public SkillData LastSkill;
        public SerializableDictionary<Element, float> ElementDamageRateMultiplier;
        [Range(0, 1)] public float DropItemRate = 0;
        [ShowIf("@DropItemRate > 0")] public Table<ItemData> DropItemTable;
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
    public class EnemySkillData
    {
        [Required] public SkillData Skill;
        [MinValue(0)] public int CoolTime;
    }
}