#nullable enable
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Domain.Model
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Item")]
    public class ItemData : ScriptableObject, IHasInfo
    {
        [Required] public Sprite Icon;
        public Rarity Rarity;
        public int Price = 100;
        public bool EffectsOnUse = true;
        public bool EffectsOnThrow = false;

        [ShowIf("@EffectsOnUse && EffectsOnThrow")] [SerializeField]
        public bool IsSameSkill = false;

        [ShowIf("EffectsOnUse")] public SkillDataOnUse SkillOnUse;
        [ShowIf("EffectsOnThrow")] public SkillDataOnThrow SkillOnThrow;
        [ShowIf("_usable")] [MinValue(1)] public int UsageLimit;
        [ReadOnly] [Required] private string _name = "";

        public ItemData(string name, Sprite icon, Rarity rarity, bool effectsOnUse, bool effectsOnThrow,
            SkillDataOnUse skillOnUse, SkillDataOnThrow skillOnThrow, int usageLimit)
        {
            _name = name;
            Icon = icon;
            Rarity = rarity;
            EffectsOnUse = effectsOnUse;
            EffectsOnThrow = effectsOnThrow;
            SkillOnUse = skillOnUse;
            SkillOnThrow = skillOnThrow;
            UsageLimit = usageLimit;
        }

        public string Name => $"<color=#{ColorUtility.ToHtmlStringRGB(Rarity.GetColor())}>{_name}</color>";
        private bool _usable => EffectsOnUse || EffectsOnThrow;
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            _name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.SaveAssets();

            if (!(EffectsOnUse && EffectsOnThrow))
            {
                IsSameSkill = false;
            }

            if (IsSameSkill && SkillOnUse != null)
            {
                SkillOnThrow = new SkillDataOnThrow(SkillOnUse.Area, SkillOnUse.Effect);
            }
        }
#endif
        public string Info()
        {
            var info = $"{Name}\n";
            if (_usable)
            {
                if (IsSameSkill)
                {
                    info += $"[使用・投擲時]\n{SkillOnUse.Info()}\n";
                }
                else
                {
                    if (EffectsOnUse)
                    {
                        info += $"[使用時]\n{SkillOnUse.Info()}\n";
                    }

                    if (EffectsOnThrow)
                    {
                        info += $"[投擲時]\n{SkillOnThrow.Info()}\n";
                    }
                }

                info += $"使用可能回数: {UsageLimit}";
            }

            return info;
        }
    }
}