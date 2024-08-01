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
        public int Price;
        public NullableSerializable<SkillMemento> SkillOnUse;
        public NullableSerializable<SkillMemento> SkillOnThrow;
        public bool UseOnDeath;
        public int MaxUsages;
        public int RemainingUsages;
        [SerializeReference] public IConditionData[] Conditions;
    }
}