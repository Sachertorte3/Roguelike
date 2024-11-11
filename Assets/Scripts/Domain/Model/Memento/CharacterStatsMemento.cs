using System;
using System.Collections.Generic;
using Domain.Model.Effect;
using UnityEngine;
using Utilities.Serialize;
using Utilities.Stats;

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
        [SerializeField] private SerializableDictionary<string, StatData> _conditionResistance;
        public Dictionary<string, StatData> ConditionResistance => _conditionResistance.ToDictionary();
        [field: SerializeField] public StatData ViewRange { get; private set; }
        [field: SerializeField] public ResourceData WaitTime { get; private set; }

        public CharacterStatsMemento(ResourceData hp, StatData hpNaturalRecovery,
            Dictionary<Element, StatData> elementAttackMultiplier,
            Dictionary<Element, StatData> elementDamageRateMultiplier, Dictionary<string, StatData> conditionResistance,
            StatData viewRange, ResourceData waitTime)
        {
            Hp = hp;
            HpNaturalRecoveryAmount = hpNaturalRecovery;
            _elementAttackMultiplier = elementAttackMultiplier.ToSerializable();
            _elementDamageRateMultiplier = elementDamageRateMultiplier.ToSerializable();
            _conditionResistance = conditionResistance.ToSerializable();
            ViewRange = viewRange;
            WaitTime = waitTime;
        }
    }
}