#nullable enable
using System;

namespace Domain.Model.Character
{
    [Serializable]
    public class CharacterSkillMemento
    {
        public SpawnEffectSkillMemento Skill;
        public int CoolTime;
        public int RemainingTurn;
    }
}