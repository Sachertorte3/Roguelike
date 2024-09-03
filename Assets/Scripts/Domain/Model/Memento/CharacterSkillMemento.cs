#nullable enable
using System;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterSkillMemento
    {
        public SpawnEffectSkillMemento Skill;
        public int CoolTime;
        public int RemainingTurn;
    }
}