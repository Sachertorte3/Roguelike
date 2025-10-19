#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterSkillWithRuleMemento
    {
        [field: SerializeField] public CharacterSkillMemento Skill;
        [field: SerializeField] public int Priority;

        public CharacterSkillWithRuleMemento(CharacterSkillMemento skill, int priority)
        {
            Skill = skill;
            Priority = priority;
        }
    }
}