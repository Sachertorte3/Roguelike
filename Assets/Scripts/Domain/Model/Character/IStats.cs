using R3;

namespace Domain.Model.Characters
{
    public interface IStats
    {
        public ReadOnlyReactiveProperty<int> HpValue { get; }
        public ReadOnlyReactiveProperty<int> MaxHp { get; }
    }
}