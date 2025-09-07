#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Condition;
using Domain.Model.Item;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class BaseItemMemento
    {
        [SerializeField] private string _id;
        public Id<IItem> Id => new Id<IItem>(_id);
        [field: SerializeField] public string BaseName { get; private set; }
        [field: SerializeField] public Option<string> CustomName { get; private set; }
        [SerializeField] private string _iconName;
        public Sprite Icon => ScriptableObjectLoader.LoadIcon(_iconName);
        [field: SerializeField] public bool IsShiny { get; private set; }
        [field: SerializeField] public int AdditionalPrice { get; private set; }
        [field: SerializeField] public float MultiplyPrice { get; private set; }
        [field: SerializeField] public ItemState State { get; private set; }
        [SerializeField] private List<string> _upgradePaths;
        public List<UpgradePath> UpgradePaths => _upgradePaths.Select(path => new UpgradePath(path)).ToList();
        [field: SerializeField] public int MaxUsages { get; private set; }
        [field: SerializeField] public int RemainingUsages { get; private set; }
        [field: SerializeField] public bool IsCursed { get; private set; }
        [field: SerializeField] public bool IsCurseIdentified { get; private set; }
        [field: SerializeField] public int UpgradeLimit { get; private set; }
        [field: SerializeReference] public List<IConditionData> Conditions { get; private set; }
        public BaseItemMemento(
            Id<IItem> id,
            string baseName,
            Option<string> customName,
            Sprite icon,
            bool isShiny,
            int additionalPrice,
            float multiplyPrice,
            ItemState state,
            List<UpgradePath> upgradePaths,
            int maxUsages,
            int remainingUsages,
            bool isCursed,
            bool isCurseIdentified,
            int upgradeLimit,
            List<IConditionData> conditions)
        {
            _id = id.ToString();
            BaseName = baseName;
            CustomName = customName;
            _iconName = icon.name;
            IsShiny = isShiny;
            AdditionalPrice = additionalPrice;
            MultiplyPrice = multiplyPrice;
            State = state;
            _upgradePaths = upgradePaths.Select(path => path.ToString()).ToList();
            MaxUsages = maxUsages;
            RemainingUsages = remainingUsages;
            IsCursed = isCursed;
            IsCurseIdentified = isCurseIdentified;
            UpgradeLimit = upgradeLimit;
            Conditions = conditions;
        }

        public BaseItemMemento CopyWith(
            Id<IItem>? id = null,
            string? baseName = null,
            Option<string>? customName = null,
            Sprite? icon = null,
            bool? isShiny = null,
            int? additionalPrice = null,
            float? multiplyPrice = null,
            ItemState? state = null,
            List<UpgradePath>? upgradePaths = null,
            int? maxUsages = null,
            int? remainingUsages = null,
            bool? isCursed = null,
            bool? isCurseIdentified = null,
            int? upgradeLimit = null,
            List<IConditionData>? conditions = null)
        {
            return new BaseItemMemento(
                id ?? Id,
                baseName ?? BaseName,
                customName ?? CustomName,
                icon ?? Icon,
                isShiny ?? IsShiny,
                additionalPrice ?? AdditionalPrice,
                multiplyPrice ?? MultiplyPrice,
                state ?? State,
                upgradePaths ?? UpgradePaths,
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