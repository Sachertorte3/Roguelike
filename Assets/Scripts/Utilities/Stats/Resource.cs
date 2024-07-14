using System;
using R3;
using UnityEngine;

namespace Stats
{
    public class Resource : IDisposable
    {
        public readonly IntStat _max;
        private readonly ReactiveProperty<int> _value;

        public Resource(int maxValue)
        {
            _max = new IntStat(maxValue);
            _value = new ReactiveProperty<int>(maxValue);
            MaxValue.Subscribe(_ => clampCurrentValue());
        }

        public Resource(int maxValue, int value)
        {
            _max = new IntStat(maxValue);
            _value = new ReactiveProperty<int>(value);
            MaxValue.Subscribe(_ => clampCurrentValue());
        }

        public Resource(ResourceData data)
        {
            _max = new IntStat(data.Max);
            _value = new ReactiveProperty<int>(data.Value);
            MaxValue.Subscribe(_ => clampCurrentValue());
        }

        public ReadOnlyReactiveProperty<int> MaxValue => _max.Value;
        public ReadOnlyReactiveProperty<int> Value => _value;

        public void Dispose()
        {
            _value.Dispose();
        }

        ~Resource()
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

        public int Lose(int value, string name)
        {
            if (value < 0)
            {
                return -Gain(-value, name);
            }

            var oldValue = Value.CurrentValue;
            _value.Value = Mathf.Clamp(Value.CurrentValue - value, 0, MaxValue.CurrentValue);
            return oldValue - _value.Value;
        }

        public int Gain(int value, string name)
        {
            if (value < 0)
            {
                return -Lose(-value, name);
            }

            var oldValue = Value.CurrentValue;
            _value.Value = Mathf.Clamp(Value.CurrentValue + value, 0, MaxValue.CurrentValue);
            return _value.Value - oldValue;
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