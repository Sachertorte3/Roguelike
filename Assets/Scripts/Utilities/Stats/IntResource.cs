using System;
using R3;
using UnityEngine;

namespace Stats
{
    public class IntResource : IDisposable
    {
        public readonly IntStat _max;
        private readonly ReactiveProperty<int> _value;

        public IntResource(int maxValue)
        {
            _max = new IntStat(maxValue);
            _value = new ReactiveProperty<int>(maxValue);
            MaxValue.Subscribe(_ => clampCurrentValue());
        }

        public IntResource(int maxValue, int value)
        {
            _max = new IntStat(maxValue);
            _value = new ReactiveProperty<int>(value);
            MaxValue.Subscribe(_ => clampCurrentValue());
        }

        public IntResource(ResourceData data)
        {
            _max = new IntStat(data.Max);
            _value = new ReactiveProperty<int>(Mathf.RoundToInt(data.Value));
            MaxValue.Subscribe(_ => clampCurrentValue());
        }

        public ReadOnlyReactiveProperty<int> MaxValue => _max.Value;
        public ReadOnlyReactiveProperty<int> Value => _value.Select(v => Mathf.RoundToInt(v)).ToReadOnlyReactiveProperty();

        public void Dispose()
        {
            _value.Dispose();
        }

        ~IntResource()
        {
            Dispose();
        }

        public ResourceData GetData()
        {
            return new ResourceData(_max.GetData(), _value.Value);
        }

        private void clampCurrentValue()
        {
            _value.Value = Mathf.Clamp(Value.CurrentValue, 0, MaxValue.CurrentValue);
        }

        public int Lose(int value)
        {
            if (value < 0)
            {
                throw new ArgumentException("Value cannot be negative");
            }
            var oldValue = Value.CurrentValue;
            _value.Value = Mathf.Clamp(Value.CurrentValue - value, 0, MaxValue.CurrentValue);
            return oldValue - Value.CurrentValue;
        }

        public int Gain(int value)
        {
            if (value < 0)
            {
                throw new ArgumentException("Value cannot be negative");
            }
            var oldValue = Value.CurrentValue;
            _value.Value = Mathf.Clamp(Value.CurrentValue + value, 0, MaxValue.CurrentValue);
            return Value.CurrentValue - oldValue;
        }

        public void AddMaxValue(float value)
        {
            _max.AddValue(value);
        }

        public void AddMaxMultiplier(float value)
        {
            _max.AddMultiplier(value);
        }

        public void RemoveMaxValue(float value)
        {
            _max.AddValue(-value);
        }

        public void RemoveMaxMultiplier(float value)
        {
            _max.AddMultiplier(-value);
        }
    }
}