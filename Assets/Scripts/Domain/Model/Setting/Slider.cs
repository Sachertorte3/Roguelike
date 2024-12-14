#nullable enable
using R3;

namespace Domain.Model.Setting
{
    public record Slider : IOptionInput
    {
        public readonly int Max;
        public readonly int Min;
        public readonly string Name;
        public readonly ReactiveProperty<bool> IsEnabled;

        public Slider(string name, int min, int max, int defaultValue, ReactiveProperty<bool>? isEnabled = null)
        {
            Name = name;
            Min = min;
            Max = max;
            Value = new ReactiveProperty<int>(defaultValue);
            IsEnabled = isEnabled ?? new ReactiveProperty<bool>(true);
        }

        public ReactiveProperty<int> Value { get; init; }
        public int CurrentValue => Value.CurrentValue;
    }
}