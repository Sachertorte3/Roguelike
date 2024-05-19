using R3;
using StatSystem;
using System;
using Utilities;

namespace Model.Characters.Stats
{
    internal class CharacterStats : IDisposable, IStats
    {
        private readonly Stat _strength;

        public CharacterStats(int maxHp, int strength)
        {
            Hp = new Resource(maxHp);
            _strength = new Stat(strength);
            Strength = _strength.ToReactiveProperty();
        }

        public Resource Hp { get; init; }

        public void Dispose()
        {
            Hp.Dispose();
        }

        public ReadOnlyReactiveProperty<int> HpValue => Hp.Value;
        public ReadOnlyReactiveProperty<int> MaxHp => Hp.Max;
        public ReadOnlyReactiveProperty<int> Strength { get; init; }
    }

    public interface IStats
    {
        public ReadOnlyReactiveProperty<int> HpValue { get; }
        public ReadOnlyReactiveProperty<int> MaxHp { get; }
        public ReadOnlyReactiveProperty<int> Strength { get; }
    }
}