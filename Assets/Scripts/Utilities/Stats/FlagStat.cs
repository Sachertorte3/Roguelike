using R3;

namespace Stats
{
    public class FlagStat
    {
        private readonly ReactiveProperty<int> _flags;
        private readonly ReactiveProperty<bool> _value = new();
        public bool CurrentValue => _flags.Value > 0;
        public int CurrentFlags => _flags.Value;

        public FlagStat(int flags)
        {
            _flags = new ReactiveProperty<int>(flags);
        }

        public ReadOnlyReactiveProperty<bool> Value => _value;
        public ReadOnlyReactiveProperty<int> Flags => _flags;

        public void AddFlags()
        {
            _flags.Value++;
            _value.Value = _flags.Value > 0;
        }

        public void RemoveFlags()
        {
            _flags.Value--;
            _value.Value = _flags.Value > 0;
        }
    }
}