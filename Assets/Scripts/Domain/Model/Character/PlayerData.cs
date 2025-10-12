#nullable enable
using Domain.Model.Character.Status;
using Domain.Model.Character.Type;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Serialize;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Domain.Model.Character
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [ReadOnly][Required] public string Name = "";
        [SerializeField] public Human CharacterType;
        [PreviewField(Alignment = ObjectFieldAlignment.Left), ReadOnly] public Sprite _sprite;
        public bool IsBoss;
        [MinValue(1)] public int Hp;
        public MoveSpeed MoveSpeed = MoveSpeed.Normal;
        [MinValue(0)] public int InventoryCapacity = 20;
        public bool HasAllConditionProof;
        public bool IsHard;
        public bool IsHeavy;
        public bool IsFlying;
        public bool CanThroughWalls;
        public CharacterSkillData DefaultSkill;
        public SerializableDictionary<Element, float> ElementAttackMultiplier;
        public SerializableDictionary<Element, float> ElementDamageRateMultiplier;
        public SerializableDictionary<ConditionTemplate, float> ConditionResistance;
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Name = Path.GetFileNameWithoutExtension(assetPath);
            EditorUtility.SetDirty(this);

            DefaultSkill.Skill.OnValidate(CommonSenseParameters.SkillOnUseProbabilityOfSuccess);

            EditorUtility.SetDirty(this);
        }
#endif
    }
}