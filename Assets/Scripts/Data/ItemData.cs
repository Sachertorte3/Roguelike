#nullable enable
using UnityEngine;
using Sirenix.OdinInspector;

namespace Scripts.Data
{
#if UNITY_EDITOR
    using System.IO;
    using UnityEditor;
#endif
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Item")]
    public class ItemData : ScriptableObject, IHasInfo
    {
        [ReadOnly] public string Name = "";
        public Sprite Icon;
        public SkillData Skill;
        [MinValue(1)] public int UsageLimit;
#if UNITY_EDITOR
        private void OnValidate()
        {
            string assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.SaveAssets();
        }
#endif
        public string Info() => $"{Name}\n{Skill.Info()}\n使用可能回数: {UsageLimit}";
    }
}
