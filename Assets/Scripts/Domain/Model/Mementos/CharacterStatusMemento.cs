using System;
using Stats;

namespace Domain.Model.Character
{
    [Serializable]
    public class CharacterStatusMemento
    {
        public ResourceData Hp;
        public StatData HpNaturalRecoveryAmount;
        public StatData AttackMultiplier;
        public StatData ViewRange;
        public ResourceData WaitTime;
        public int ClairvoyantFlags;
        public ConditionMemento[] Conditions;
    }
}