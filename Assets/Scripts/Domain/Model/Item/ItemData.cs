#nullable enable

using System.Collections.Generic;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Domain.Model.Item
{
    public interface IItemData : IHasRarity
    {
    }
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Item")]
    public class ItemData : ScriptableObject, IItemData
    {
        [SerializeField] public ItemCategory Category;
        [Required] public Sprite Icon;
        public bool IsShiny;
        public bool CannotUseIfCursed => Category != ItemCategory.Weapons;
        public bool CannotDropIfCursed => Category == ItemCategory.Weapons;
        public bool IdentifyIfGot => Category == ItemCategory.Weapons;
        public bool IdentifyIfUsed => Category != ItemCategory.Wands;
        public bool AutoDestroyWhenDisabled => Category == ItemCategory.Potions || Category == ItemCategory.Scrolls;
        [SerializeField] private Rarity _rarity;
        public Rarity Rarity => _rarity;
        public int AdditionalPrice = 0;
        public float MultiplyPrice = 1f;
        public ItemEffectType EffectType = ItemEffectType.SpawnEffect;

        #region spawn effect

        [ShowIf("SpawnEffectsOnUse")] public bool UseOnDeath;

        [ShowIf("@EffectType == ItemEffectType.SpawnEffect")]
        public bool SpawnEffectsOnUse = true;

        [ShowIf("@EffectType == ItemEffectType.SpawnEffect")]
        public bool SpawnEffectsOnThrow;

        [ShowIf("@SpawnEffectsOnUse && SpawnEffectsOnThrow && !IsSameSkill")]
        public bool IsSameEffect;

        [ShowIf("@SpawnEffectsOnUse && SpawnEffectsOnThrow")]
        public bool IsSameSkill;

        [ShowIf("SpawnEffectsOnUse")] public SkillDataOnUse? SkillOnUse;
        [ShowIf("SpawnEffectsOnThrow")] public SkillDataOnThrow? SkillOnThrow;

        #endregion

        #region item target

        [ShowIf("@EffectType == ItemEffectType.ItemTarget")]
        [SerializeReference]
        [Required]
        public IItemEffect? ItemEffect;

        #endregion

        #region inventory target
        [ShowIf("@EffectType == ItemEffectType.InventoryTarget")]
        [SerializeReference]
        [Required]
        public IInventoryEffect? InventoryEffect;
        #endregion

        [ShowIf("_usable")][MinValue(1)] public int UsageLimit;
        public int UpgradeLimit = 3;
        [SerializeReference] public List<IConditionData> PassiveConditions;
        [SerializeField] private List<StringSerializableDirectWeaponFeature> _featuresToMergeWeapon;
        public List<DirectWeaponFeature> FeaturesToMergeWeapon => _featuresToMergeWeapon.Select(feature => feature.Value).ToList();


        private bool _usable => EffectType switch
        {
            ItemEffectType.SpawnEffect => SpawnEffectsOnUse || SpawnEffectsOnThrow,
            ItemEffectType.ItemTarget => ItemEffect != null,
            ItemEffectType.InventoryTarget => InventoryEffect != null,
            _ => false
        };

        public void AddEffects(List<IEffect> effects)
        {
            if (SkillOnUse != null)
            {
                SkillOnUse.Effects.AddRange(effects);
            }

            if (SkillOnThrow != null)
            {
                SkillOnThrow.Effects.AddRange(effects);
            }
        }
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
                SkillOnThrow.SetSameEffect(SkillOnUse);
            }

            if (IsSameSkill && SkillOnUse != null)
            {
                if (SkillOnThrow == null)
                {
                    SkillOnThrow =
                        new SkillDataOnThrow(SkillOnUse.Area, SkillOnUse.Effects,
                            CommonSenseParameters.SkillOnThrowProbabilityOfSuccess);
                }
                else
                {
                    SkillOnThrow =
                        new SkillDataOnThrow(SkillOnUse.Area, SkillOnUse.Effects, SkillOnThrow.ProbabilityOfSuccess);
                }
            }

            if (SkillOnUse != null)
            {
                SkillOnUse.OnValidate();
            }

            if (SkillOnThrow != null)
            {
                SkillOnThrow.OnValidate();
            }

            EditorUtility.SetDirty(this);
        }
#endif
    }
}