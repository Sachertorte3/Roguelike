using System;
using Domain.Model.Characters;
using R3;
using Stats;

namespace Domain.Service.Characters.Stats
{
    internal class CharacterStats : IDisposable, IStats
    {
        public CharacterStats(ResourceData hp, StatData hpNaturalRecoveryAmount, StatData viewRange)
        {
            Hp = new Resource(hp);
            HpNaturalRecoveryAmount = new IntStat(hpNaturalRecoveryAmount);
            ViewRange = new Stat(viewRange);
        }

        public Resource Hp { get; init; }
        public IntStat HpNaturalRecoveryAmount { get; init; }
        public Stat ViewRange { get; init; }

        public void Dispose()
        {
            Hp.Dispose();
            HpNaturalRecoveryAmount.Dispose();
            ViewRange.Dispose();
        }

        public ReadOnlyReactiveProperty<int> HpValue => Hp.Value;
        public int CurrentHp => Hp.Value.CurrentValue;
        public ReadOnlyReactiveProperty<int> MaxHp => Hp.MaxValue;
        public int CurrentMaxHp => Hp.MaxValue.CurrentValue;
        public ReadOnlyReactiveProperty<int> HpNaturalRecoveryAmountValue => HpNaturalRecoveryAmount.Value;
        public int CurrentHpNaturalRecoveryAmount => HpNaturalRecoveryAmount.CurrentValue;
        public ReadOnlyReactiveProperty<float> ViewRangeValue => ViewRange.Value;
        public float CurrentViewRange => ViewRange.CurrentValue;
    }
}