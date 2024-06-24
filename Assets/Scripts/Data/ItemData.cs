#nullable enable
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using Effect;
using Data.Area;
using Data.Effect;


#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Item")]
    public class ItemData : ScriptableObject, IHasInfo
    {
        [ReadOnly, Required] private string _name = "";
        public string Name => $"<color=#{ColorUtility.ToHtmlStringRGB(Rarity.GetColor())}>{_name}</color>";
        [Required] public Sprite Icon;
        public Rarity Rarity;
        public bool EffectsOnUse = true;
        public bool EffectsOnThrow = false;

        [ShowIf("@EffectsOnUse && EffectsOnThrow")] [SerializeField]
        private bool _isSameSkill = false;

        [ShowIf("EffectsOnUse")] public SkillDataOnUse SkillOnUse;
        [ShowIf("EffectsOnThrow")] public SkillDataOnThrow SkillOnThrow;
        [ShowIf("Usable")] [MinValue(1)] public int UsageLimit;
        private bool Usable => EffectsOnUse || EffectsOnThrow;

        public ItemData(string name, Sprite icon, Rarity rarity, bool effectsOnUse, bool effectsOnThrow, SkillDataOnUse skillOnUse, SkillDataOnThrow skillOnThrow, int usageLimit)
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
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            _name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.SaveAssets();

            if (!(EffectsOnUse && EffectsOnThrow))
            {
                _isSameSkill = false;
            }

            if (_isSameSkill && SkillOnUse != null)
            {
                SkillOnThrow = new SkillDataOnThrow(SkillOnUse.Area, SkillOnUse.Effect);
            }
        }
#endif
        public string Info()
        {
            var info = $"{Name}\n";
            if (Usable)
            {
                if (_isSameSkill)
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

    [Serializable]
    public class SkillDataOnUse : IHasInfo
    {
        [SerializeReference, Required] public IArea Area;
        [SerializeReference, Required] public IEffect Effect;
        [SerializeReference, Required] public IEffectPosition Position;

        public SkillDataOnUse(IEffectPosition position, IArea area, IEffect effect)
        {
            Position = position;
            Area = area;
            Effect = effect;
        }

        public string Info()
        {
            return $"効果: {Effect.Info()}\n発動位置: {Position.Info()}\n範囲: {Area.Info()}";
        }
    }

    [Serializable]
    public class SkillDataOnThrow : IHasInfo
    {
        [SerializeReference, Required] public IArea Area;
        [SerializeReference, Required] public IEffect Effect;

        public SkillDataOnThrow(IArea area, IEffect effect)
        {
            Area = area;
            Effect = effect;
        }

        public string Info()
        {
            return $"効果: {Effect.Info()}\n範囲: {Area.Info()}";
        }
    }
}