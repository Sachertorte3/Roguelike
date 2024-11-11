#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ItemMemento
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public ItemCategory Category { get; private set; }
        [field: SerializeField] public string BaseName { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string IconName { get; private set; }
        [field: SerializeField] public bool IsShiny { get; private set; }
        [field: SerializeField] public ItemState State { get; private set; }
        [field: SerializeField] public List<string> UpgradePaths { get; private set; }
        [field: SerializeField] public Option<ISkillMemento> SkillOnUse { get; private set; }
        [field: SerializeField] public Option<ISkillMemento> SkillOnThrow { get; private set; }
        [field: SerializeField] public bool HasSameEffect { get; private set; }
        [field: SerializeField] public bool HasSameSkill { get; private set; }
        [field: SerializeField] public bool UseOnDeath { get; private set; }
        [field: SerializeField] public int MaxUsages { get; private set; }
        [field: SerializeField] public int RemainingUsages { get; private set; }
        [field: SerializeField] public bool IsCursed { get; private set; }
        [field: SerializeField] public bool CannotUseIfCursed { get; private set; }
        [field: SerializeField] public bool CannotDropIfCursed { get; private set; }
        [field: SerializeField] public bool IdentifyIfGot { get; private set; }
        [field: SerializeField] public bool IdentifyIfUsed { get; private set; }
        [field: SerializeField] public bool IsCurseIdentified { get; private set; }
        [field: SerializeField] public bool AutoDestroyWhenDisabled { get; private set; }
        [field: SerializeField] public int UpgradeLimit { get; private set; }
        [field: SerializeReference] public IConditionData[] Conditions { get; private set; }

        public ItemMemento(
            string id, ItemCategory category, string baseName, string name, string iconName, bool isShiny,
            ItemState state,
            List<string> upgradePaths, Option<ISkillMemento> skillOnUse, Option<ISkillMemento> skillOnThrow,
            bool hasSameEffect, bool hasSameSkill, bool useOnDeath, int maxUsages, int remainingUsages,
            bool isCursed, bool cannotUseIfCursed, bool cannotDropIfCursed, bool identifyIfGot, bool identifyIfUsed,
            bool isCurseIdentified, bool autoDestroyWhenDisabled, int upgradeLimit, IConditionData[] conditions)
        {
            Id = id;
            Category = category;
            BaseName = baseName;
            Name = name;
            IconName = iconName;
            IsShiny = isShiny;
            State = state;
            UpgradePaths = upgradePaths;
            SkillOnUse = skillOnUse;
            SkillOnThrow = skillOnThrow;
            HasSameEffect = hasSameEffect;
            HasSameSkill = hasSameSkill;
            UseOnDeath = useOnDeath;
            MaxUsages = maxUsages;
            RemainingUsages = remainingUsages;
            IsCursed = isCursed;
            CannotUseIfCursed = cannotUseIfCursed;
            CannotDropIfCursed = cannotDropIfCursed;
            IdentifyIfGot = identifyIfGot;
            IdentifyIfUsed = identifyIfUsed;
            IsCurseIdentified = isCurseIdentified;
            AutoDestroyWhenDisabled = autoDestroyWhenDisabled;
            UpgradeLimit = upgradeLimit;
            Conditions = conditions;
        }

        public ItemMemento CopyWith(
            string? id = null, ItemCategory? category = null, string? baseName = null, string? name = null,
            string? iconName = null, bool? isShiny = null, ItemState? state = null,
            List<string>? upgradePaths = null, Option<ISkillMemento>? skillOnUse = null,
            Option<ISkillMemento>? skillOnThrow = null,
            bool? hasSameEffect = null, bool? hasSameSkill = null, bool? useOnDeath = null, int? maxUsages = null,
            int? remainingUsages = null,
            bool? isCursed = null, bool? cannotUseIfCursed = null, bool? cannotDropIfCursed = null,
            bool? identifyIfGot = null, bool? identifyIfUsed = null, bool? isCurseIdentified = null,
            bool? autoDestroyWhenDisabled = null, int? upgradeLimit = null, IConditionData[]? conditions = null)
        {
            return new ItemMemento(
                id ?? Id,
                category ?? Category,
                baseName ?? BaseName,
                name ?? Name,
                iconName ?? IconName,
                isShiny ?? IsShiny,
                state ?? State,
                upgradePaths ?? UpgradePaths,
                skillOnUse ?? SkillOnUse,
                skillOnThrow ?? SkillOnThrow,
                hasSameEffect ?? HasSameEffect,
                hasSameSkill ?? HasSameSkill,
                useOnDeath ?? UseOnDeath,
                maxUsages ?? MaxUsages,
                remainingUsages ?? RemainingUsages,
                isCursed ?? IsCursed,
                cannotUseIfCursed ?? CannotUseIfCursed,
                cannotDropIfCursed ?? CannotDropIfCursed,
                identifyIfGot ?? IdentifyIfGot,
                identifyIfUsed ?? IdentifyIfUsed,
                isCurseIdentified ?? IsCurseIdentified,
                autoDestroyWhenDisabled ?? AutoDestroyWhenDisabled,
                upgradeLimit ?? UpgradeLimit,
                conditions ?? Conditions
            );
        }
    }
}