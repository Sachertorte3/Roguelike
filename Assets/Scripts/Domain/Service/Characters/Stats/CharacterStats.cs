using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Memento;
using R3;
using Stats;
using Utilities;

namespace Domain.Service.Characters.Stats
{
    internal class CharacterStats : IDisposable, ISerializable<CharacterStatsMemento>, IStats
    {
        public CharacterStats(CharacterStatsMemento memento)
        {
            Hp = new IntResource(memento.Hp);
            HpNaturalRecoveryAmount = new Stat(memento.HpNaturalRecoveryAmount);
            ElementAttackMultiplier =
                memento.ElementAttackMultiplier.ToDictionary(pair => pair.Key, pair => new Stat(pair.Value));
            ElementDamageRateMultiplier =
                memento.ElementDamageRateMultiplier.ToDictionary(pair => pair.Key, pair => new Stat(pair.Value));
            ViewRange = new Stat(memento.ViewRange);
            WaitTime = new Resource(memento.WaitTime);
            foreach (Element element in Enum.GetValues(typeof(Element)))
            {
                if (!ElementAttackMultiplier.ContainsKey(element))
                {
                    ElementAttackMultiplier[element] = new Stat(1);
                }

                if (!ElementDamageRateMultiplier.ContainsKey(element))
                {
                    ElementDamageRateMultiplier[element] = new Stat(1);
                }
            }
            ConditionResistance = memento.ConditionResistance.ToDictionary(pair => pair.Key, pair => new Stat(pair.Value));
        }

        public CharacterStatsMemento Serialize()
        {
            return new CharacterStatsMemento
            (
                Hp.GetData(),
                HpNaturalRecoveryAmount.GetData(),
                ElementAttackMultiplier.ToDictionary(pair => pair.Key, pair => pair.Value.GetData()),
                ElementDamageRateMultiplier.ToDictionary(pair => pair.Key, pair => pair.Value.GetData()),
                ConditionResistance.ToDictionary(pair => pair.Key, pair => pair.Value.GetData()),
                ViewRange.GetData(),
                WaitTime.GetData()
            );
        }

        public static CharacterStatsMemento Build(int maxHp, float hpNaturalRecoveryAmount,
            Dictionary<Element, float> elementAttackMultiplier, Dictionary<Element, float> elementDamageRateMultiplier,
            Dictionary<ConditionTemplate, float> conditionResistance, float viewRange, float waitTime)
        {
            return new CharacterStatsMemento
            (
                new ResourceData(new StatData(maxHp), maxHp),
                new StatData(hpNaturalRecoveryAmount),
                elementAttackMultiplier.ToDictionary(pair => pair.Key, pair => new StatData(pair.Value)),
                elementDamageRateMultiplier.ToDictionary(pair => pair.Key, pair => new StatData(pair.Value)),
                conditionResistance.ToDictionary(pair => pair.Key.name, pair => new StatData(pair.Value)),
                new StatData(viewRange),
                new ResourceData(new StatData(waitTime), waitTime)
            );
        }

        public IntResource Hp { get; init; }
        public Stat HpNaturalRecoveryAmount { get; init; }
        public Stat ViewRange { get; init; }
        public Resource WaitTime { get; init; }
        public Dictionary<Element, Stat> ElementAttackMultiplier { get; init; }
        public Dictionary<Element, Stat> ElementDamageRateMultiplier { get; init; }
        public Dictionary<string, Stat> ConditionResistance { get; init; }
        public void Dispose()
        {
            Hp.Dispose();
            HpNaturalRecoveryAmount.Dispose();
            ViewRange.Dispose();
            WaitTime.Dispose();
            foreach (var element in ElementAttackMultiplier.Values)
            {
                element.Dispose();
            }

            foreach (var element in ElementDamageRateMultiplier.Values)
            {
                element.Dispose();
            }

            foreach (var condition in ConditionResistance.Values)
            {
                condition.Dispose();
            }
        }

        public ReadOnlyReactiveProperty<int> HpValue => Hp.Value;
        public int CurrentHp => Hp.Value.CurrentValue;
        public ReadOnlyReactiveProperty<int> MaxHp => Hp.MaxValue;
        public int CurrentMaxHp => Hp.MaxValue.CurrentValue;
        public ReadOnlyReactiveProperty<float> HpNaturalRecoveryAmountValue => HpNaturalRecoveryAmount.Value;
        public float CurrentHpNaturalRecoveryAmount => HpNaturalRecoveryAmount.CurrentValue;
        public ReadOnlyReactiveProperty<float> ViewRangeValue => ViewRange.Value;
        public float CurrentViewRange => ViewRange.CurrentValue;
        public float CurrentMaxWaitTime => WaitTime.MaxValue.CurrentValue;
        public float CurrentWaitTime => WaitTime.Value.CurrentValue;

        public float GetStatValue(StatType type)
        {
            return type switch
            {
                StatType.MaxHp => CurrentMaxHp,
                StatType.HpNaturalRecovery => CurrentHpNaturalRecoveryAmount,
                StatType.ViewRange => CurrentViewRange,
                StatType.WaitTime => CurrentWaitTime,
                _ => throw new ArgumentException($"Invalid stat type: {type}")
            };
        }

        public float GetElementAttackMultiplier(Element element)
        {
            return ElementAttackMultiplier[element].CurrentValue;
        }

        public float GetElementDamageRateMultiplier(Element element)
        {
            return ElementDamageRateMultiplier[element].CurrentValue;
        }

        public float GetConditionResistance(ConditionTemplate condition)
        {
            if (ConditionResistance.ContainsKey(condition.name))
            {
                return ConditionResistance[condition.name].CurrentValue;
            }
            return 0f;
        }

        public void AddStatValue(StatType type, float value)
        {
            switch (type)
            {
                case StatType.MaxHp:
                    Hp.AddMaxValue(value);
                    break;
                case StatType.HpNaturalRecovery:
                    HpNaturalRecoveryAmount.AddValue(value);
                    break;
                case StatType.ViewRange:
                    ViewRange.AddValue(value);
                    break;
                case StatType.WaitTime:
                    WaitTime.AddMaxValue(value);
                    break;
            }
        }

        public void AddElementAttackMultiplier(Element element, float value)
        {
            ElementAttackMultiplier[element].AddValue(value);
        }

        public void AddElementDamageRateMultiplier(Element element, float value)
        {
            ElementDamageRateMultiplier[element].AddValue(value);
        }

        public void RemoveStatValue(StatType type, float value)
        {
            AddStatValue(type, -value);
        }

        public void RemoveElementAttackMultiplier(Element element, float value)
        {
            ElementAttackMultiplier[element].AddValue(-value);
        }

        public void RemoveElementDamageRateMultiplier(Element element, float value)
        {
            ElementDamageRateMultiplier[element].AddValue(-value);
        }

        public void AddStatMultiplier(StatType type, float value)
        {
            switch (type)
            {
                case StatType.MaxHp:
                    Hp.AddMaxMultiplier(value);
                    break;
                case StatType.HpNaturalRecovery:
                    HpNaturalRecoveryAmount.AddMultiplier(value);
                    break;
                case StatType.ViewRange:
                    ViewRange.AddMultiplier(value);
                    break;
                case StatType.WaitTime:
                    WaitTime.AddMaxMultiplier(value);
                    break;
            }
        }

        public void AddElementAttackMultiplierMultiplier(Element element, float value)
        {
            ElementAttackMultiplier[element].AddMultiplier(value);
        }

        public void AddElementDamageRateMultiplierMultiplier(Element element, float value)
        {
            ElementDamageRateMultiplier[element].AddMultiplier(value);
        }

        public void RemoveStatMultiplier(StatType type, float value)
        {
            AddStatMultiplier(type, -value);
        }

        public void RemoveElementAttackMultiplierMultiplier(Element element, float value)
        {
            AddElementAttackMultiplierMultiplier(element, -value);
        }

        public void RemoveElementDamageRateMultiplierMultiplier(Element element, float value)
        {
            AddElementDamageRateMultiplierMultiplier(element, -value);
        }

        public void MultiplyStat(StatType type, float value)
        {
            switch (type)
            {
                case StatType.MaxHp:
                    Hp.MultiplyMaxValue(value);
                    break;
                case StatType.HpNaturalRecovery:
                    HpNaturalRecoveryAmount.Multiply(value);
                    break;
                case StatType.ViewRange:
                    ViewRange.Multiply(value);
                    break;
                case StatType.WaitTime:
                    WaitTime.MultiplyMaxValue(value);
                    break;
            }
        }

        public void MultiplyElementAttackMultiplier(Element element, float value)
        {
            ElementAttackMultiplier[element].Multiply(value);
        }

        public void MultiplyElementDamageRateMultiplier(Element element, float value)
        {
            ElementDamageRateMultiplier[element].Multiply(value);
        }

        public void DivideStat(StatType type, float value)
        {
            MultiplyStat(type, 1 / value);
        }

        public void DivideElementAttackMultiplier(Element element, float value)
        {
            MultiplyElementAttackMultiplier(element, 1 / value);
        }

        public void DivideElementDamageRateMultiplier(Element element, float value)
        {
            MultiplyElementDamageRateMultiplier(element, 1 / value);
        }
    }
}