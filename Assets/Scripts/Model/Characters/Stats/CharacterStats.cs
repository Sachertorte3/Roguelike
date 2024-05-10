using R3;
using Scripts.Utilities;
using StatSystem;

namespace Scripts.Model.Characters.Stats
{
    internal class CharacterStats: IStats
    {
        public Resource Hp { get; init; }
        public ReadOnlyReactiveProperty<int> HpValue => Hp.Value;
        public ReadOnlyReactiveProperty<int> MaxHp => Hp.Max;
        public ReadOnlyReactiveProperty<int> Strength { get; init; }
        private readonly Stat _strength;
        public CharacterStats(int maxHp, int strength)
        {
            Hp = new Resource(maxHp);
            _strength = new Stat(strength);
            Strength = _strength.ToReactiveProperty();
        }
    }
    public interface IStats
    {
        public ReadOnlyReactiveProperty<int> HpValue { get; }
        public ReadOnlyReactiveProperty<int> MaxHp { get; }
        public ReadOnlyReactiveProperty<int> Strength { get; }
    }
}
