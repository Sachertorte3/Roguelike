#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class TrapMemento
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public EntityMemento Entity { get; private set; }
        [field: SerializeReference] public SpawnEffectSkillMemento Skill { get; private set; }

        public TrapMemento(string name, EntityMemento entity, SpawnEffectSkillMemento skill)
        {
            Name = name;
            Entity = entity;
            Skill = skill;
        }
    }
}