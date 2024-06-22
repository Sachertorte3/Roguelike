#nullable enable
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using Effect;
using Data.Area;
using Data.Effect;
using Codice.Client.BaseCommands;


#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Item")]
    public class ItemData : ScriptableObject, IHasInfo
    {
        [ReadOnly] [Required] public string Name;
        [Required] public Sprite Icon;
        public bool EffectsOnUse = true;
        public bool EffectsOnThrow = false;

        [ShowIf("@EffectsOnUse && EffectsOnThrow")] [SerializeField]
        private bool _isSameSkill = false;

        [ShowIf("EffectsOnUse")] public SkillDataOnUse SkillOnUse;
        [ShowIf("EffectsOnThrow")] public SkillDataOnThrow SkillOnThrow;
        [ShowIf("Usable")] [MinValue(1)] public int UsageLimit;
        private bool Usable => EffectsOnUse || EffectsOnThrow;
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.SaveAssets();

            if (!(EffectsOnUse && EffectsOnThrow))
            {
                _isSameSkill = false;
            }

            if (_isSameSkill)
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
        [SerializeReference] [Required] public IArea Area;
        [SerializeReference] [Required] public IEffect Effect;
        [SerializeReference] [Required] public IEffectPosition Position;

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
        [SerializeReference] [Required] public IArea Area;
        [SerializeReference] [Required] public IEffect Effect;

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