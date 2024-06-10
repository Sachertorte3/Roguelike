using System;
using R3;
using StatSystem;
using Utilities;

namespace Model.Domain.Characters.Stats
{
    internal class CharacterStats : IDisposable, IStats
    {
        public CharacterStats(int maxHp, int hp)
        {
            Hp = new Resource(maxHp, hp);
        }

        public Resource Hp { get; init; }

        public void Dispose()
        {
            Hp.Dispose();
        }

        public ReadOnlyReactiveProperty<int> HpValue => Hp.Value;
        public ReadOnlyReactiveProperty<int> MaxHp => Hp.Max;
    }
}