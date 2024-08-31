using System;
using R3;
using UnityEngine;

namespace Stats
{
    public class Resource : IDisposable
    {
        private readonly Stat _max;
        private readonly ReactiveProperty<float> _value;

        public Resource(float maxValue)
        {
            _max = new Stat(maxValue);
            _value = new ReactiveProperty<float>(maxValue);
            MaxValue.Subscribe(_ => clampCurrentValue());
        }

        public Resource(int maxValue, int value)
        {
            _max = new Stat(maxValue);
            _value = new ReactiveProperty<float>(value);
            MaxValue.Subscribe(_ => clampCurrentValue());
        }

        public Resource(ResourceData data)
        {
            _max = new Stat(data.Max);
            _value = new ReactiveProperty<float>(data.Value);
            MaxValue.Subscribe(_ => clampCurrentValue());
        }

        public ReadOnlyReactiveProperty<float> MaxValue => _max.Value;
        public ReadOnlyReactiveProperty<float> Value => _value;

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

        public void Set(float value)
        {
            _value.Value = Mathf.Clamp(value, 0, MaxValue.CurrentValue);
        }

        public float Lose(float value)
        {
            if (value < 0)
            {
                return -Gain(-value);
            }

            var oldValue = Value.CurrentValue;
            _value.Value = Mathf.Clamp(Value.CurrentValue - value, 0, MaxValue.CurrentValue);
            return oldValue - _value.Value;
        }

        public float Gain(float value)
        {
            if (value < 0)
            {
                return -Lose(-value);
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

        public void MultiplyMaxValue(float value)
        {
            _max.Multiply(value);
        }

        public void RemoveMaxValue(float value)
        {
            _max.AddValue(-value);
        }

        public void RemoveMaxMultiplier(float value)
        {
            _max.AddMultiplier(-value);
        }
        
        public void DivideMaxValue(float value)
        {
            _max.Multiply(1 / value);
        }

        public bool IsFull()
        {
            return Value.CurrentValue >= MaxValue.CurrentValue;
        }
    }
}