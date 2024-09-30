using System;
using System.Collections.Generic;
using Domain.Model.Character;
using Stats;
using UnityEngine;
using Utilities;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterStatsMemento
    {
        [field: SerializeField] public ResourceData Hp { get; private set; }
        [field: SerializeField] public StatData HpNaturalRecoveryAmount { get; private set; }
        [SerializeField] private SerializableDictionary<Element, StatData> _elementAttackMultiplier;
        public Dictionary<Element, StatData> ElementAttackMultiplier => _elementAttackMultiplier;
        [SerializeField] private SerializableDictionary<Element, StatData> _elementDamageRateMultiplier;
        public Dictionary<Element, StatData> ElementDamageRateMultiplier => _elementDamageRateMultiplier;
        [field: SerializeField] public StatData ViewRange { get; private set; }
        [field: SerializeField] public ResourceData WaitTime { get; private set; }
        public CharacterStatsMemento(ResourceData hp, StatData hpNaturalRecovery, Dictionary<Element, StatData> elementAttackMultiplier, Dictionary<Element, StatData> elementDamageRateMultiplier, StatData viewRange, ResourceData waitTime)
        {
            Hp = hp;
            HpNaturalRecoveryAmount = hpNaturalRecovery;
            _elementAttackMultiplier = elementAttackMultiplier.ToSerializable();
            _elementDamageRateMultiplier = elementDamageRateMultiplier.ToSerializable();
            ViewRange = viewRange;
            WaitTime = waitTime;
        }
    }
}