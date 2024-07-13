using System;
using Domain.Model.Characters;
using R3;
using Stats;

namespace Domain.Service.Characters.Stats
{
    internal class CharacterStats : IDisposable, IStats
    {
        public CharacterStats(int maxHp, int hp, float viewRange)
        {
            Hp = new Resource(maxHp, hp);
            ViewRange = new Stat(viewRange);
        }

        public Resource Hp { get; init; }
        public Stat ViewRange { get; init; }

        public void Dispose()
        {
            Hp.Dispose();
            ViewRange.Dispose();
        }

        public ReadOnlyReactiveProperty<int> HpValue => Hp.Value;
        public int CurrentHp => Hp.Value.CurrentValue;
        public ReadOnlyReactiveProperty<int> MaxHp => Hp.MaxValue;
        public int CurrentMaxHp => Hp.MaxValue.CurrentValue;
        public ReadOnlyReactiveProperty<float> ViewRangeValue => ViewRange.Value;
        public float CurrentViewRange => ViewRange.CurrentValue;
    }
}