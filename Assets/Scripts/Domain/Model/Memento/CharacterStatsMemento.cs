using System;
using Domain.Model.Character;
using Stats;

namespace Domain.Model.Memento
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