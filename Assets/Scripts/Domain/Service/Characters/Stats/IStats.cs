using R3;

namespace Domain.Service.Characters.Stats
{
    public interface IStats
    {
        public ReadOnlyReactiveProperty<int> HpValue { get; }
        public ReadOnlyReactiveProperty<int> MaxHp { get; }
    }
}