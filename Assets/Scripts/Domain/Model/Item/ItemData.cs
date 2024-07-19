#nullable enable
using Sirenix.OdinInspector;
using UnityEngine;
using Domain.Model.Effect;
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Condition;





#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Domain.Model.Item
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Item")]
    public class ItemData : ScriptableObject, IHasInfo, IHasRarity
    {
        [Required] public Sprite Icon;
        [SerializeField] private Rarity _rarity;
        public Rarity Rarity => _rarity;
        public int Price = 100;
        public bool EffectsOnUse = true;
        public bool EffectsOnThrow = false;

        [ShowIf("@EffectsOnUse && EffectsOnThrow")]
        [SerializeField]
        public bool IsSameSkill = false;

        [ShowIf("EffectsOnUse")] public SkillDataOnUse? SkillOnUse;
        [ShowIf("EffectsOnThrow")] public SkillDataOnThrow? SkillOnThrow;
        [ShowIf("_usable")][MinValue(1)] public int UsageLimit;
        [SerializeReference] public List<IConditionData> PassiveConditions;
        [ReadOnly][Required] private string _name = "";

        public ItemData(string name, Sprite icon, Rarity rarity,
            SkillDataOnUse? skillOnUse, SkillDataOnThrow? skillOnThrow, int usageLimit, List<IConditionData> conditions)
        {
            _name = name;
            Icon = icon;
            _rarity = rarity;
            EffectsOnUse = skillOnUse != null;
            EffectsOnThrow = skillOnThrow != null;
            SkillOnUse = skillOnUse;
            SkillOnThrow = skillOnThrow;
            UsageLimit = usageLimit;
            PassiveConditions = conditions;
        }

        public string Name => _name.SetColored(Rarity.GetColor());
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

                info += $"使用可能回数: {UsageLimit}\n";
            }

            foreach (var condition in PassiveConditions)
            {
                info += $"パッシブ効果: {condition.Name}\n";
            }

            return info;
        }
    }

    public interface IHasRarity
    {
        Rarity Rarity { get; }
    }
}