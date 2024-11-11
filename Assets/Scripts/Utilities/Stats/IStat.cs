using R3;

namespace Stats
{
    public interface IStat
    {
        public ReadOnlyReactiveProperty<float> Value { get; }
        public float CurrentValue { get; }
    }
}