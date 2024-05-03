using R3;
using Scripts.Utilities;
using StatSystem;

namespace Scripts.Model.Characters.Stats
{
    public class CharacterStats
    {
        public readonly Resource Hp;
        public ReadOnlyReactiveProperty<int> MaxHp => Hp.Max;
        public readonly ReadOnlyReactiveProperty<int> Strength;
        private readonly Stat _strength;
        public CharacterStats(int maxHp, int strength)
        {
            Hp = new Resource(maxHp);
            _strength = new Stat(strength);
            Strength = _strength.ToReactiveProperty();
        }
    }
}
