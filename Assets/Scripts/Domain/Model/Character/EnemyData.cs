#nullable enable
using Domain.Model.Character.Type;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Domain.Model.Effect;
using System;


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
        [SerializeReference] public ICharacterType CharacterType;
        public bool IsBoss = false;
        [MinValue(1)] public int Hp;
        public Aggression Aggression = Aggression.AvoidAllies;
        public bool WanderAround = true;
        public MoveSpeed MoveSpeed = MoveSpeed.Normal;
        public SkillData[] Skills;
        public bool HasLastSkill = false;
        [ShowIf("@HasLastSkill")] public SkillData LastSkill;
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.SaveAssets();
        }
#endif
    }
    public enum MoveSpeed
    {
        Quarter,
        Half,
        Normal,
        Double,
        Quadruple
    }
    public static class MoveSpeedExtensions
    {
        public static float ToWaitTime(this MoveSpeed moveSpeed)
        {
            return moveSpeed switch
            {
                MoveSpeed.Quarter => 4,
                MoveSpeed.Half => 2,
                MoveSpeed.Normal => 1,
                MoveSpeed.Double => 0.5f,
                MoveSpeed.Quadruple => 0.25f,
                _ => throw new ArgumentException("Invalid MoveSpeed")
            };
        }
    }
}