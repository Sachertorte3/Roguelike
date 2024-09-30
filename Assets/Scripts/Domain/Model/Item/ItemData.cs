#nullable enable
using System.Collections.Generic;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Item
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Item")]
    public class ItemData : ScriptableObject, IHasInfo, IHasRarity
    {
        [Required] public Sprite Icon;
        public bool IsShiny;
        [SerializeField] private Rarity _rarity;
        public Rarity Rarity => _rarity;
        public ItemEffectType EffectType = ItemEffectType.SpawnEffect;
        #region spawn effect
        [ShowIf("SpawnEffectsOnUse")] public bool UseOnDeath = false;
        [ShowIf("@EffectType == ItemEffectType.SpawnEffect")] public bool SpawnEffectsOnUse = true;
        [ShowIf("@EffectType == ItemEffectType.SpawnEffect")] public bool SpawnEffectsOnThrow = false;

        [ShowIf("@SpawnEffectsOnUse && SpawnEffectsOnThrow && !IsSameSkill")]
        public bool IsSameEffect;
        [ShowIf("@SpawnEffectsOnUse && SpawnEffectsOnThrow")]
        public bool IsSameSkill;

        [ShowIf("SpawnEffectsOnUse")] public SkillDataOnUse? SkillOnUse;
        [ShowIf("SpawnEffectsOnThrow")] public SkillDataOnThrow? SkillOnThrow;
        #endregion
        #region item target
        [ShowIf("@EffectType == ItemEffectType.ItemTarget"), SerializeReference, Required] public IItemEffect? ItemEffect;
        #endregion
        [ShowIf("_usable")][MinValue(1)] public int UsageLimit;
        [SerializeReference] public List<IConditionData> PassiveConditions;

        private ItemData(string itemName, Sprite icon, bool isShiny, Rarity rarity, bool useOnDeath, int usageLimit, List<IConditionData> conditions)
        {
            name = itemName;
            Icon = icon;
            IsShiny = isShiny;
            _rarity = rarity;
            UseOnDeath = useOnDeath;
            UsageLimit = usageLimit;
            PassiveConditions = conditions;
        }
        public ItemData(string itemName, Sprite icon, bool isShiny, Rarity rarity,
            SkillDataOnUse? skillOnUse, SkillDataOnThrow? skillOnThrow, bool isSameEffect, bool isSameSkill, bool useOnDeath, int usageLimit, List<IConditionData> conditions)
            : this(itemName, icon, isShiny, rarity, useOnDeath, usageLimit, conditions)
        {
            EffectType = ItemEffectType.SpawnEffect;
            SpawnEffectsOnUse = skillOnUse != null;
            SpawnEffectsOnThrow = skillOnThrow != null;
            IsSameEffect = isSameEffect;
            IsSameSkill = isSameSkill;
            SkillOnUse = skillOnUse;
            SkillOnThrow = skillOnThrow;
        }
        public ItemData(string itemName, Sprite icon, bool isShiny, Rarity rarity, IItemEffect itemEffect, bool useOnDeath, int usageLimit, List<IConditionData> conditions)
        : this(itemName, icon, isShiny, rarity, useOnDeath, usageLimit, conditions)
        {
            EffectType = ItemEffectType.ItemTarget;
            ItemEffect = itemEffect;
        }

        public string Name => name.SetColored(Rarity.GetColor());
        private bool _usable => SpawnEffectsOnUse || SpawnEffectsOnThrow;
#if UNITY_EDITOR
        private void OnValidate()
        {
            switch (EffectType)
            {
                case ItemEffectType.None:
                    SpawnEffectsOnUse = false;
                    SpawnEffectsOnThrow = false;
                    ItemEffect = null;
                    break;
                case ItemEffectType.SpawnEffect:
                    ItemEffect = null;
                    break;
                case ItemEffectType.ItemTarget:
                    SpawnEffectsOnUse = false;
                    SpawnEffectsOnThrow = false;
                    break;
            }

            if (!SpawnEffectsOnUse)
            {
                UseOnDeath = false;
            }

            if (!(SpawnEffectsOnUse && SpawnEffectsOnThrow))
            {
                IsSameEffect = false;
                IsSameSkill = false;
            }

            if (IsSameEffect && SkillOnUse != null && SkillOnThrow != null)
            {
                SkillOnThrow.Effect = SkillOnUse.Effect;
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
}