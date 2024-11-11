using R3;

namespace Utilities.Stats
{
    public interface IStat
    {
        public ReadOnlyReactiveProperty<float> Value { get; }
        public float CurrentValue { get; }
    }
}