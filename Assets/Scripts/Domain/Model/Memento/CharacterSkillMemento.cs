#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterSkillMemento
    {
        [field: SerializeField] public SpawnEffectSkillMemento Skill { get; private set; }
        [field: SerializeField] public int CoolTime { get; private set; }
        [field: SerializeField] public int RemainingTurn { get; private set; }
        public CharacterSkillMemento(SpawnEffectSkillMemento skill, int coolTime, int remainingTurn)
        {
            Skill = skill;
            CoolTime = coolTime;
            RemainingTurn = remainingTurn;
        }
    }
}