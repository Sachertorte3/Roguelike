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
        public string Id;
        public string Name;
        public string IconName;
        public bool IsShiny;
        public ItemState State;
        public List<string> UpgradePaths;
        public Option<ISkillMemento> SkillOnUse;
        public Option<ISkillMemento> SkillOnThrow;
        public bool HasSameEffect;
        public bool HasSameSkill;
        public bool UseOnDeath;
        public int MaxUsages;
        public int RemainingUsages;
        [SerializeReference] public IConditionData[] Conditions;
    }
}