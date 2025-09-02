#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Item;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class DirectWeaponMemento : IItemMemento
    {
        [SerializeField] private string _id;
        public Id<IItem> Id => new Id<IItem>(_id);
        [field: SerializeField] public string BaseName { get; private set; }
        [field: SerializeField] public string RevealedName { get; private set; }
        [field: SerializeField] public Option<string> CustomName { get; private set; }
        [SerializeField] private string _iconName;
        public Sprite Icon => ScriptableObjectLoader.LoadIcon(_iconName);
        [field: SerializeField] public bool IsShiny { get; private set; }
        [field: SerializeField] public ItemState State { get; private set; }
        [field: SerializeField] public List<string> UpgradePaths { get; private set; }
        [field: SerializeField] public Option<WeaponPrefix> Prefix { get; private set; }
        [field: SerializeField] public List<ElementPower> ElementPowers { get; private set; }
        [field: SerializeField] public List<DirectWeaponFeature> Features { get; private set; }
        [field: SerializeField] public SpawnEffectSkillMemento SkillOnUse { get; private set; }
        [field: SerializeField] public SpawnEffectSkillMemento SkillOnThrow { get; private set; }
        [field: SerializeField] public int MaxUsages { get; private set; }
        [field: SerializeField] public int RemainingUsages { get; private set; }
        [field: SerializeField] public bool IsCursed { get; private set; }
        [field: SerializeField] public bool IsCurseIdentified { get; private set; }
        [field: SerializeField] public int UpgradeLimit { get; private set; }
        [field: SerializeReference] public List<IConditionData> Conditions { get; private set; }

        public DirectWeaponMemento(
            Id<IItem> id,
            string baseName,
            string revealedName,
            Option<string> customName,
            Sprite icon,
            bool isShiny,
            ItemState state,
            List<string> upgradePaths,
            Option<WeaponPrefix> prefix,
            List<ElementPower> elementPowers,
            List<DirectWeaponFeature> features,
            SpawnEffectSkillMemento skillOnUse,
            SpawnEffectSkillMemento skillOnThrow,
            int maxUsages,
            int remainingUsages,
            bool isCursed,
            bool isCurseIdentified,
            int upgradeLimit,
            List<IConditionData> conditions)
        {
            _id = id.ToString();
            BaseName = baseName;
            RevealedName = revealedName;
            CustomName = customName;
            _iconName = icon.name;
            IsShiny = isShiny;
            State = state;
            UpgradePaths = upgradePaths;
            Prefix = prefix;
            ElementPowers = elementPowers;
            Features = features;
            SkillOnUse = skillOnUse;
            SkillOnThrow = skillOnThrow;
            MaxUsages = maxUsages;
            RemainingUsages = remainingUsages;
            IsCursed = isCursed;
            IsCurseIdentified = isCurseIdentified;
            UpgradeLimit = upgradeLimit;
            Conditions = conditions;
        }

        public DirectWeaponMemento CopyWith(
            Id<IItem>? id = null,
            string? baseName = null,
            string? revealedName = null,
            Option<string>? customName = null,
            Sprite? icon = null,
            bool? isShiny = null,
            ItemState? state = null,
            List<string>? upgradePaths = null,
            Option<WeaponPrefix>? prefix = null,
            List<ElementPower>? elementPowers = null,
            List<DirectWeaponFeature>? features = null,
            SpawnEffectSkillMemento? skillOnUse = null,
            SpawnEffectSkillMemento? skillOnThrow = null,
            int? maxUsages = null,
            int? remainingUsages = null,
            bool? isCursed = null,
            bool? isCurseIdentified = null,
            int? upgradeLimit = null,
            List<IConditionData>? conditions = null)
        {
            return new DirectWeaponMemento(
                id ?? Id,
                baseName ?? BaseName,
                revealedName ?? RevealedName,
                customName ?? CustomName,
                icon ?? Icon,
                isShiny ?? IsShiny,
                state ?? State,
                upgradePaths ?? UpgradePaths,
                prefix ?? Prefix,
                elementPowers ?? ElementPowers,
                features ?? Features,
                skillOnUse ?? SkillOnUse,
                skillOnThrow ?? SkillOnThrow,
                maxUsages ?? MaxUsages,
                remainingUsages ?? RemainingUsages,
                isCursed ?? IsCursed,
                isCurseIdentified ?? IsCurseIdentified,
                upgradeLimit ?? UpgradeLimit,
                conditions ?? Conditions
            );
        }
    }
}