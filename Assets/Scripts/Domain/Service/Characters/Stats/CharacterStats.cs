using System;
using Domain.Model.Character;
using Domain.Model.Condition;
using R3;
using Stats;

namespace Domain.Service.Characters.Stats
{
    internal class CharacterStats : IDisposable, IStats
    {
        public CharacterStats(ResourceData hp, StatData hpNaturalRecoveryAmount, StatData attackMultiplier, StatData viewRange, ResourceData waitTime)
        {
            Hp = new IntResource(hp);
            HpNaturalRecoveryAmount = new IntStat(hpNaturalRecoveryAmount);
            AttackMultiplier = new Stat(attackMultiplier);
            ViewRange = new Stat(viewRange);
            WaitTime = new Resource(waitTime);
        }

        public IntResource Hp { get; init; }
        public IntStat HpNaturalRecoveryAmount { get; init; }
        public Stat AttackMultiplier { get; init; }
        public Stat ViewRange { get; init; }
        public Resource WaitTime { get; init; }

        public void Dispose()
        {
            Hp.Dispose();
            HpNaturalRecoveryAmount.Dispose();
            AttackMultiplier.Dispose();
            ViewRange.Dispose();
            WaitTime.Dispose();
        }

        public ReadOnlyReactiveProperty<int> HpValue => Hp.Value;
        public int CurrentHp => Hp.Value.CurrentValue;
        public ReadOnlyReactiveProperty<int> MaxHp => Hp.MaxValue;
        public int CurrentMaxHp => Hp.MaxValue.CurrentValue;
        public ReadOnlyReactiveProperty<int> HpNaturalRecoveryAmountValue => HpNaturalRecoveryAmount.Value;
        public int CurrentHpNaturalRecoveryAmount => HpNaturalRecoveryAmount.CurrentValue;
        public ReadOnlyReactiveProperty<float> AttackMultiplierValue => AttackMultiplier.Value;
        public float CurrentAttackMultiplier => AttackMultiplier.CurrentValue;
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
                StatType.AttackMultiplier => CurrentAttackMultiplier,
                StatType.ViewRange => CurrentViewRange,
                StatType.WaitTime => CurrentWaitTime,
                _ => throw new ArgumentException($"Invalid stat type: {type}"),
            };
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
                case StatType.AttackMultiplier:
                    AttackMultiplier.AddValue(value);
                    break;
                case StatType.ViewRange:
                    ViewRange.AddValue(value);
                    break;
                case StatType.WaitTime:
                    WaitTime.AddMaxValue(value);
                    break;
            }
        }

        public void RemoveStatValue(StatType type, float value)
        {
            AddStatValue(type, -value);
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
                case StatType.AttackMultiplier:
                    AttackMultiplier.AddMultiplier(value);
                    break;
                case StatType.ViewRange:
                    ViewRange.AddMultiplier(value);
                    break;
                case StatType.WaitTime:
                    WaitTime.AddMaxMultiplier(value);
                    break;
            }
        }

        public void RemoveStatMultiplier(StatType type, float value)
        {
            AddStatMultiplier(type, -value);
        }
    }
}