using R3;
using UnityEngine;

namespace Data.Setting
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
            OnValueChanged = new ReactiveProperty<int>(defaultValue);
        }

        public int Value => OnValueChanged.Value;
        public ReactiveProperty<int> OnValueChanged { get; }

        public void SetValue(int value)
        {
            OnValueChanged.Value = Mathf.Clamp(value, Min, Max);
        }
    }
}