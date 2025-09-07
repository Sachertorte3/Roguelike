#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{

    [Serializable]
    public class ItemMemento : IItemMemento
    {
        [field: SerializeField] public BaseItemMemento BaseItem { get; private set; }
        [field: SerializeField] public ItemCategory Category { get; private set; }
        [field: SerializeField] public Option<ISkillMemento> SkillOnUse { get; private set; }
        [field: SerializeField] public Option<ISkillMemento> SkillOnThrow { get; private set; }
        [field: SerializeField] public bool HasSameEffect { get; private set; }
        [field: SerializeField] public bool HasSameSkill { get; private set; }
        [field: SerializeField] public bool UseOnDeath { get; private set; }
        [field: SerializeField] public bool CannotUseIfCursed { get; private set; }
        [field: SerializeField] public bool CannotDropIfCursed { get; private set; }
        [field: SerializeField] public bool IdentifyIfGot { get; private set; }
        [field: SerializeField] public bool IdentifyIfUsed { get; private set; }
        [field: SerializeField] public bool AutoDestroyWhenDisabled { get; private set; }
        [field: SerializeField] public List<DirectWeaponFeature> FeaturesToMergeWeapon { get; private set; }

        public ItemMemento(
            BaseItemMemento baseItem,
            ItemCategory category,
            Option<ISkillMemento> skillOnUse,
            Option<ISkillMemento> skillOnThrow,
            bool hasSameEffect,
            bool hasSameSkill,
            bool useOnDeath,
            bool cannotUseIfCursed,
            bool cannotDropIfCursed,
            bool identifyIfGot,
            bool identifyIfUsed,
            bool autoDestroyWhenDisabled,
            List<DirectWeaponFeature> featuresToMergeWeapon)
        {
            BaseItem = baseItem;
            Category = category;
            SkillOnUse = skillOnUse;
            SkillOnThrow = skillOnThrow;
            HasSameEffect = hasSameEffect;
            HasSameSkill = hasSameSkill;
            UseOnDeath = useOnDeath;
            CannotUseIfCursed = cannotUseIfCursed;
            CannotDropIfCursed = cannotDropIfCursed;
            IdentifyIfGot = identifyIfGot;
            IdentifyIfUsed = identifyIfUsed;
            AutoDestroyWhenDisabled = autoDestroyWhenDisabled;
            FeaturesToMergeWeapon = featuresToMergeWeapon;
        }

        public ItemMemento CopyWith(
            BaseItemMemento? baseItem = null,
            ItemCategory? category = null,
            Option<ISkillMemento>? skillOnUse = null,
            Option<ISkillMemento>? skillOnThrow = null,
            bool? hasSameEffect = null,
            bool? hasSameSkill = null,
            bool? useOnDeath = null,
            bool? cannotUseIfCursed = null,
            bool? cannotDropIfCursed = null,
            bool? identifyIfGot = null,
            bool? identifyIfUsed = null,
            bool? autoDestroyWhenDisabled = null,
            List<DirectWeaponFeature>? featuresToMergeWeapon = null)
        {
            return new ItemMemento(
                baseItem ?? BaseItem,
                category ?? Category,
                skillOnUse ?? SkillOnUse,
                skillOnThrow ?? SkillOnThrow,
                hasSameEffect ?? HasSameEffect,
                hasSameSkill ?? HasSameSkill,
                useOnDeath ?? UseOnDeath,
                cannotUseIfCursed ?? CannotUseIfCursed,
                cannotDropIfCursed ?? CannotDropIfCursed,
                identifyIfGot ?? IdentifyIfGot,
                identifyIfUsed ?? IdentifyIfUsed,
                autoDestroyWhenDisabled ?? AutoDestroyWhenDisabled,
                featuresToMergeWeapon ?? FeaturesToMergeWeapon
            );
        }
    }
}