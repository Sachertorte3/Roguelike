#nullable enable
using System;
using Domain.Model.Condition;
using Domain.Model.Item;
using UnityEngine;

namespace Domain.Model.Character
{
    [Serializable]
    public class ItemMemento
    {
        public int Id;
        public string Name;
        public string IconName;
        public ItemState State;
        public int UpgradeCount;
        public int Price;
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