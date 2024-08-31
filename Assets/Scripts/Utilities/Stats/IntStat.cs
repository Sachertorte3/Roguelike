using System;
using R3;
using UnityEngine;

namespace Stats
{
    public class IntStat : IDisposable
    {
        private readonly Stat _stat;
        public readonly ReadOnlyReactiveProperty<int> Value;
        public int CurrentValue => Mathf.RoundToInt(_stat.CurrentValue);

        public IntStat(float baseValue)
        {
            _stat = new Stat(baseValue);
            Value = _stat.Value.Select(x => Mathf.RoundToInt(x)).ToReadOnlyReactiveProperty();
        }

        public IntStat(StatData data)
        {
            _stat = new Stat(data);
            Value = _stat.Value.Select(x => Mathf.RoundToInt(x)).ToReadOnlyReactiveProperty();
        }

        public void Dispose()
        {
            _stat.Dispose();
        }

        ~IntStat()
        {
            Dispose();
        }

        public StatData GetData()
        {
            return _stat.GetData();
        }

        public void AddValue(float value)
        {
            _stat.AddValue(value);
        }

        public void AddMultiplier(float multiplier)
        {
            _stat.AddMultiplier(multiplier);
        }

        public void Multiply(float multiplier)
        {
            _stat.Multiply(multiplier);
        }
    }
}