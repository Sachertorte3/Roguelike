using System.Collections;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Stats
{
    public class Stat
    {
        private float _baseValue;
        private float _additiveValue = 0;
        private float _multiplicativeValue = 1;
        private ReactiveProperty<float> _value = new();
        public ReadOnlyReactiveProperty<float> Value => _value;

        public Stat(float baseValue)
        {
            _baseValue = baseValue;
            _additiveValue = 0f;
            _multiplicativeValue = 1f;
            SetValue();
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