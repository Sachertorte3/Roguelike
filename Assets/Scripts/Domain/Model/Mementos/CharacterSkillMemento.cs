#nullable enable
using System;

namespace Domain.Model.Character
{
    [Serializable]
    public class CharacterSkillMemento
    {
        public SkillMemento Skill;
        public int CoolTime;
        public int RemainingTurn;
    }
}