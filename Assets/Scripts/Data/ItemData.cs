#nullable enable
using Sirenix.OdinInspector;
using System.IO;
using UnityEngine;

namespace Data
{
    using System;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Item")]
    public class ItemData : ScriptableObject, IHasInfo
    {
        [ReadOnly, Required] public string Name = "";
        [Required] public Sprite Icon;
        public bool EffectsOnUse = true;
        public bool EffectsOnThrow;
        [ShowIf("Usable")] public SkillData Skill;
        [ShowIf("Usable")] [MinValue(1)] public int UsageLimit;
        private bool Usable => EffectsOnUse || EffectsOnThrow;
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.SaveAssets();

            if (!Usable)
            {
                Skill = null;
            }
        }
#endif
        public string Info()
        {
            var info = Name;
            if (Usable)
            {
                info += "\n[" + (EffectsOnUse, EffectsOnThrow) switch
                {
                    (true, true) => "使用・投擲時",
                    (true, false) => "使用時",
                    (false, true) => "投擲時",
                    (false, false) => throw new InvalidOperationException()
                };
                info += $"]\n{Skill.Info()}\n使用可能回数: {UsageLimit}";
            }

            return info;
        }
    }
}