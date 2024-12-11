using R3;
using UnityEngine;

namespace Domain.Model.Setting
{
    public record Slider : IOptionInput
    {
        public readonly int Max;
        public readonly int Min;
        public readonly string Name;

        public Slider(string name, int min, int max, int defaultValue)
        {
            Name = name;
            Min = min;
            Max = max;
            _value = new ReactiveProperty<int>(defaultValue);
        }

        private readonly ReactiveProperty<int> _value;
        public ReadOnlyReactiveProperty<int> Value => _value;
        public int CurrentValue => Value.CurrentValue;

        public void SetValue(int value)
        {
            _value.Value = Mathf.Clamp(value, Min, Max);
        }
    }
}