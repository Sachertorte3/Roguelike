using System;
using R3;
using Stats;
using Unity.Logging;
using UnityEngine;

namespace Model.Domain.Characters.Stats
{
    internal class Resource : IDisposable
    {
        public readonly Stat _max;
        private readonly ReactiveProperty<int> _value;
        public readonly ReadOnlyReactiveProperty<int> Max;

        public Resource(int maxValue)
        {
            _max = new Stat(maxValue);
            Max = _max.Value.Select(v => Mathf.RoundToInt(v)).ToReadOnlyReactiveProperty();
            _value = new ReactiveProperty<int>(maxValue);
            Max.Subscribe(_ => clampCurrentValue());
        }

        public Resource(int maxValue, int value)
        {
            _max = new Stat(maxValue);
            Max = _max.Value.Select(v => Mathf.RoundToInt(v)).ToReadOnlyReactiveProperty();
            _value = new ReactiveProperty<int>(value);
            Max.Subscribe(_ => clampCurrentValue());
        }

        public ReadOnlyReactiveProperty<int> Value => _value;

        public void Dispose()
        {
            _value.Dispose();
        }

        private void clampCurrentValue()
        {
            _value.Value = Mathf.Clamp(Value.CurrentValue, 0, Max.CurrentValue);
        }

        public int Lose(int value, string name)
        {
            if (value < 0)
            {
                return Gain(-value, name);
            }

            var oldValue = Value.CurrentValue;
            _value.Value = Mathf.Clamp(Value.CurrentValue - value, 0, Max.CurrentValue);
            Log.Debug($"{name} Lose {value}, current value {_value.Value}");
            return oldValue - _value.Value;
        }

        public int Gain(int value, string name)
        {
            if (value < 0)
            {
                return Lose(-value, name);
            }

            var oldValue = Value.CurrentValue;
            _value.Value = Mathf.Clamp(Value.CurrentValue + value, 0, Max.CurrentValue);
            return oldValue - _value.Value;
        }

        public void AddMaxHpValue(float value)
        {
            _max.AddValue(value);
        }
        public void AddMaxHpMultiplier(float value)
        {
            _max.AddMultiplier(value);
        }
        public void RemoveMaxHpValue(float value)
        {
            _max.AddValue(-value);
        }
        public void RemoveMaxHpMultiplier(float value)
        {
            _max.AddMultiplier(-value);
        }
    }
}