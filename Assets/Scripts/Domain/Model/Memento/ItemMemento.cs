#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ItemMemento : IItemMemento
    {
        [SerializeField] private string _id;
        public Id<IItem> Id => new Id<IItem>(_id);
        [field: SerializeField] public ItemCategory Category { get; private set; }
        [field: SerializeField] public string BaseName { get; private set; }
        [field: SerializeField] public Option<string> CustomName { get; private set; }
        [SerializeField] private string _iconName;
        public Sprite Icon => ScriptableObjectLoader.LoadIcon(_iconName);
        [field: SerializeField] public bool IsShiny { get; private set; }
        [field: SerializeField] public ItemState State { get; private set; }
        [field: SerializeField] public List<string> UpgradePaths { get; private set; }
        [field: SerializeField] public Option<ISkillMemento> SkillOnUse { get; private set; }
        [field: SerializeField] public Option<ISkillMemento> SkillOnThrow { get; private set; }
        [field: SerializeField] public bool HasSameEffect { get; private set; }
        [field: SerializeField] public bool HasSameSkill { get; private set; }
        [field: SerializeField] public bool UseOnDeath { get; private set; }
        [field: SerializeField] public Option<StorageMemento> Storage { get; private set; }
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
        [field: SerializeReference] public List<IConditionData> Conditions { get; private set; }

        public ItemMemento(
            Id<IItem> id,
            ItemCategory category,
            string baseName,
            Option<string> customName,
            Sprite icon,
            bool isShiny,
            ItemState state,
            List<string> upgradePaths,
            Option<ISkillMemento> skillOnUse,
            Option<ISkillMemento> skillOnThrow,
            bool hasSameEffect,
            bool hasSameSkill,
            bool useOnDeath,
            Option<StorageMemento> storage,
            int maxUsages,
            int remainingUsages,
            bool isCursed,
            bool cannotUseIfCursed,
            bool cannotDropIfCursed,
            bool identifyIfGot,
            bool identifyIfUsed,
            bool isCurseIdentified,
            bool autoDestroyWhenDisabled,
            int upgradeLimit,
            List<IConditionData> conditions)
        {
            _id = id.ToString();
            Category = category;
            BaseName = baseName;
            CustomName = customName;
            _iconName = icon.name;
            IsShiny = isShiny;
            State = state;
            UpgradePaths = upgradePaths;
            SkillOnUse = skillOnUse;
            SkillOnThrow = skillOnThrow;
            HasSameEffect = hasSameEffect;
            HasSameSkill = hasSameSkill;
            UseOnDeath = useOnDeath;
            Storage = storage;
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
            Id<IItem>? id = null,
            ItemCategory? category = null,
            string? baseName = null,
            Option<string>? customName = null,
            Sprite? icon = null,
            bool? isShiny = null,
            ItemState? state = null,
            List<string>? upgradePaths = null,
            Option<ISkillMemento>? skillOnUse = null,
            Option<ISkillMemento>? skillOnThrow = null,
            bool? hasSameEffect = null,
            bool? hasSameSkill = null,
            bool? useOnDeath = null,
            Option<StorageMemento>? storage = null,
            int? maxUsages = null,
            int? remainingUsages = null,
            bool? isCursed = null,
            bool? cannotUseIfCursed = null,
            bool? cannotDropIfCursed = null,
            bool? identifyIfGot = null,
            bool? identifyIfUsed = null,
            bool? isCurseIdentified = null,
            bool? autoDestroyWhenDisabled = null,
            int? upgradeLimit = null,
            List<IConditionData>? conditions = null)
        {
            return new ItemMemento(
                id ?? Id,
                category ?? Category,
                baseName ?? BaseName,
                customName ?? CustomName,
                icon ?? Icon,
                isShiny ?? IsShiny,
                state ?? State,
                upgradePaths ?? UpgradePaths,
                skillOnUse ?? SkillOnUse,
                skillOnThrow ?? SkillOnThrow,
                hasSameEffect ?? HasSameEffect,
                hasSameSkill ?? HasSameSkill,
                useOnDeath ?? UseOnDeath,
                storage ?? Storage,
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