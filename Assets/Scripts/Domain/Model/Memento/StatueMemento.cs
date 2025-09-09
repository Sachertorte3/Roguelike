#nullable enable
using System;
using UnityEngine;
using Utilities.Stats;

namespace Domain.Model.Memento
{
    [Serializable]
    public class StatueMemento
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public EntityMemento Entity { get; private set; }
        [field: SerializeReference] public SpawnActorlessEffectSkillMemento Skill { get; private set; }
        [field: SerializeField] public ResourceData Cycle { get; private set; }
        [field: SerializeField] public int AttackToBreak { get; private set; }

        public StatueMemento(
            string name,
            EntityMemento entity,
            SpawnActorlessEffectSkillMemento skill,
            ResourceData cycle,
            int attackToBreak)
        {
            Name = name;
            Entity = entity;
            Skill = skill;
            Cycle = cycle;
            AttackToBreak = attackToBreak;
        }
    }
}