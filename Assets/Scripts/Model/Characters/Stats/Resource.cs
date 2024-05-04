using R3;
using Scripts.Utilities;
using StatSystem;
using Unity.Logging;
using UnityEngine;

namespace Scripts.Model.Characters.Stats
{
    public class Resource
    {
        public readonly ReadOnlyReactiveProperty<int> Max;
        public readonly Stat _max;
        public ReadOnlyReactiveProperty<int> Value => _value;
        private readonly ReactiveProperty<int> _value;
        public Resource(int maxValue)
        {
            _max = new Stat(maxValue);
            Max = _max.ToReactiveProperty();
            _value = new ReactiveProperty<int>(maxValue);
            Max.Subscribe(_ => clampCurrentValue());
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
            _value.Value -= Mathf.Clamp(Value.CurrentValue - value, 0, Max.CurrentValue);
            Log.Debug($"Lose {value} HP");
        }
        public void Gain(int value)
        {
            if (value < 0)
            {
                Lose(-value);
                return;
            }
            _value.Value += Mathf.Clamp(Value.CurrentValue - value, 0, Max.CurrentValue);
        }
    }
}
