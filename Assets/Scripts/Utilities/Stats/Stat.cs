using System;
using R3;

namespace Stats
{
    public interface IStat
    {
        public ReadOnlyReactiveProperty<float> Value { get; }
        public float CurrentValue { get; }
    }
    public class Stat : IDisposable
    {
        private float _baseValue;
        private float _additiveValue = 0;
        private float _multiplicativeValue = 1;
        private ReactiveProperty<float> _value = new();
        public ReadOnlyReactiveProperty<float> Value => _value;
        public float CurrentValue => _value.CurrentValue;

        public Stat(float baseValue)
        {
            _baseValue = baseValue;
            _additiveValue = 0f;
            _multiplicativeValue = 1f;
            SetValue();
        }

        public Stat(StatData data)
        {
            _baseValue = data.BaseValue;
            _additiveValue = data.AdditiveValue;
            _multiplicativeValue = data.MultiplicativeValue;
            SetValue();
        }

        public void Dispose()
        {
            _value.Dispose();
        }

        ~Stat()
        {
            Dispose();
        }

        public StatData GetData()
        {
            return new StatData(_baseValue, _additiveValue, _multiplicativeValue);
        }

        public void AddValue(float value)
        {
            _additiveValue += value;
            SetValue();
        }

        public void AddMultiplier(float multiplier)
        {
            _multiplicativeValue += multiplier;
            SetValue();
        }

        private void SetValue()
        {
            _value.Value = (_baseValue + _additiveValue) * _multiplicativeValue;
        }
    }
}