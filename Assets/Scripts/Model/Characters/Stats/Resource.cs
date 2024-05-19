using R3;
using StatSystem;
using System;
using Unity.Logging;
using UnityEngine;
using Utilities;

namespace Model.Characters.Stats
{
    internal class Resource : IDisposable
    {
        public readonly Stat _max;
        private readonly ReactiveProperty<int> _value;
        public readonly ReadOnlyReactiveProperty<int> Max;

        public Resource(int maxValue)
        {
            _max = new Stat(maxValue);
            Max = _max.ToReactiveProperty();
            _value = new ReactiveProperty<int>(maxValue);
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

        public void Lose(int value)
        {
            if (value < 0)
            {
                Gain(-value);
                return;
            }

            _value.Value = Mathf.Clamp(Value.CurrentValue - value, 0, Max.CurrentValue);
            Log.Debug($"Lose {value}, current value {_value.Value}");
        }

        public void Gain(int value)
        {
            if (value < 0)
            {
                Lose(-value);
                return;
            }

            _value.Value = Mathf.Clamp(Value.CurrentValue + value, 0, Max.CurrentValue);
        }
    }
}