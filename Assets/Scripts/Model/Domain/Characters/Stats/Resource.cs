using System;
using R3;
using StatSystem;
using Unity.Logging;
using UnityEngine;
using Utilities;

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
            Max = _max.ToReactiveProperty();
            _value = new ReactiveProperty<int>(maxValue);
            Max.Subscribe(_ => clampCurrentValue());
        }
        public Resource(int maxValue, int value)
        {
            _max = new Stat(maxValue);
            Max = _max.ToReactiveProperty();
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

        public void Lose(int value, string name)
        {
            if (value < 0)
            {
                Gain(-value, name);
                return;
            }

            _value.Value = Mathf.Clamp(Value.CurrentValue - value, 0, Max.CurrentValue);
            Log.Debug($"{name} Lose {value}, current value {_value.Value}");
        }

        public void Gain(int value, string name)
        {
            if (value < 0)
            {
                Lose(-value, name);
                return;
            }

            _value.Value = Mathf.Clamp(Value.CurrentValue + value, 0, Max.CurrentValue);
        }
    }
}