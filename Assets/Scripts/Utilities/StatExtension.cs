using R3;
using StatSystem;
using UnityEngine;

namespace Scripts.Utilities
{
    public static class StatExtension
    {
        public static Observable<int> OnValueChanged(this Stat stat)
        {
            return Observable.FromEvent(h => stat.ValueChanged += h, h => stat.ValueChanged -= h).Select(_ => Mathf.RoundToInt(stat.Value));
        }
        public static ReactiveProperty<int> ToReactiveProperty(this Stat stat)
        {
            ReactiveProperty<int> property = new(Mathf.RoundToInt(stat.Value));
            stat.OnValueChanged().Subscribe(value => property.Value = value);
            return property;
        }
    }
}
