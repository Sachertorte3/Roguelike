using R3;

namespace Model.Domain.Characters.Stats
{
    public interface IStats
    {
        public ReadOnlyReactiveProperty<int> HpValue { get; }
        public ReadOnlyReactiveProperty<int> MaxHp { get; }
    }
}