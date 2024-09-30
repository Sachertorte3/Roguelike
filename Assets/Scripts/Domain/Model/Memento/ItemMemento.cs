#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Condition;
using Domain.Model.Item;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ItemMemento
    {
        [field: SerializeField] public string Id { get; private set; }
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
        [field: SerializeReference] public IConditionData[] Conditions { get; private set; }
        public ItemMemento(string id, string name, string iconName, bool isShiny, ItemState state, List<string> upgradePaths, Option<ISkillMemento> skillOnUse, Option<ISkillMemento> skillOnThrow, bool hasSameEffect, bool hasSameSkill, bool useOnDeath, int maxUsages, int remainingUsages, IConditionData[] conditions)
        {
            Id = id;
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
            Conditions = conditions;
        }
    }
}