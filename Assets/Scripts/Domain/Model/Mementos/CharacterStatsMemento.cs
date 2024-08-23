using System;
using Stats;

namespace Domain.Model.Character
{
    [Serializable]
    public class CharacterStatsMemento
    {
        public ResourceData Hp;
        public StatData HpNaturalRecoveryAmount;
        public SerializableDictionary<Element, StatData> ElementAttackMultiplier;
        public SerializableDictionary<Element, StatData> ElementDamageRateMultiplier;
        public StatData ViewRange;
        public ResourceData WaitTime;
    }
}