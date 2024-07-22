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
        public bool SpawnEffectsOnUse = true;
        public bool SpawnEffectsOnThrow = false;
        [ShowIf("SpawnEffectsOnUse")] public bool UseOnDeath = false;

        [ShowIf("@SpawnEffectsOnUse && SpawnEffectsOnThrow")]
        [SerializeField]
        public bool IsSameSkill = false;

        [ShowIf("SpawnEffectsOnUse")] public SkillDataOnUse? SkillOnUse;
        [ShowIf("SpawnEffectsOnThrow")] public SkillDataOnThrow? SkillOnThrow;
        [ShowIf("_usable")][MinValue(1)] public int UsageLimit;
        [SerializeReference] public List<IConditionData> PassiveConditions;
        [ReadOnly][Required] private string _name = "";

        public ItemData(string name, Sprite icon, Rarity rarity,
            SkillDataOnUse? skillOnUse, SkillDataOnThrow? skillOnThrow, bool useOnDeath, int usageLimit, List<IConditionData> conditions)
        {
            _name = name;
            Icon = icon;
            _rarity = rarity;
            SpawnEffectsOnUse = skillOnUse != null;
            SpawnEffectsOnThrow = skillOnThrow != null;
            UseOnDeath = useOnDeath;
            SkillOnUse = skillOnUse;
            SkillOnThrow = skillOnThrow;
            UsageLimit = usageLimit;
            PassiveConditions = conditions;
        }

        public string Name => _name.SetColored(Rarity.GetColor());
        private bool _usable => SpawnEffectsOnUse || SpawnEffectsOnThrow;
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            _name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.SaveAssets();

            if (!SpawnEffectsOnUse)
            {
                UseOnDeath = false;
            }

            if (!(SpawnEffectsOnUse && SpawnEffectsOnThrow))
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
                    if (SpawnEffectsOnUse)
                    {
                        info += $"[使用時]\n{SkillOnUse.Info()}\n";
                    }

                    if (SpawnEffectsOnThrow)
                    {
                        info += $"[投擲時]\n{SkillOnThrow.Info()}\n";
                    }
                }

                info += $"使用可能回数: {UsageLimit}\n";
            }

            if (UseOnDeath)
            {
                info += "死亡時に自動的に使用される\n";
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