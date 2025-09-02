#nullable enable
using Domain.Model.Character.Status;
using Domain.Model.Character.Type;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Table;
using Utilities.Serialize;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Domain.Model.Character
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [ReadOnly][Required] public string Name = "";
        public CharacterGroup Group = CharacterGroup.Monster;
        [SerializeReference][Required] public ICharacterType CharacterType;
        [PreviewField(Alignment = ObjectFieldAlignment.Left), ReadOnly] public Sprite _sprite;
        public bool IsBoss;
        [MinValue(1)] public int Hp;
        public Aggression Aggression = Aggression.AvoidAllies;
        public BehaviorData Behavior;
        public MoveSpeed MoveSpeed = MoveSpeed.Normal;
        public bool IsHard;
        public bool IsHeavy;
        public bool IsFlying;
        public bool CanThroughWalls;
        public bool CanPickUp;
        public bool CanUseItem;
        public EnemySkillData[] Skills;
        public bool HasLastSkill;
        [ShowIf("@HasLastSkill")] public SkillData LastSkill;
        public SerializableDictionary<Element, float> ElementDamageRateMultiplier;
        public SerializableDictionary<ConditionTemplate, float> ConditionResistance;
        [Range(0, 1)] public float DropItemRate;
        [ShowIf("@DropItemRate > 0")] public Table<ItemData> DropItemTable;
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Name = Path.GetFileNameWithoutExtension(assetPath);
            EditorUtility.SetDirty(this);

            foreach (var skill in Skills)
            {
                skill.Skill.OnValidate(CommonSenseParameters.SkillOnUseProbabilityOfSuccess);
            }

            if (LastSkill != null)
            {
                LastSkill.OnValidate(1);
            }

            EditorUtility.SetDirty(this);
        }
#endif
    }
}