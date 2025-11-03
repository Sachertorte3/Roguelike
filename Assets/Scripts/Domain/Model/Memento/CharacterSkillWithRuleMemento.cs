#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterSkillWithRuleMemento
    {
        [field: SerializeField] public SpawnEffectSkillMemento Skill;
        [field: SerializeField] public int Priority;

        public CharacterSkillWithRuleMemento(SpawnEffectSkillMemento skill, int priority)
        {
            Skill = skill;
            Priority = priority;
        }
    }
}