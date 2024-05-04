using R3;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Scripts.Model.Setting
{
    public static class Settings
    {
        public static ReactiveProperty<int> BGMVolume => _BGMVolume.OnValueChanged;
        private static readonly Slider _BGMVolume = new Slider("BGM音量", 0, 100, 50);
        public static ReactiveProperty<int> SEVolume => _SEVolume.OnValueChanged;
        private static readonly Slider _SEVolume = new Slider("SE音量", 0, 100, 50);
        public static ReactiveProperty<int> MoveMilliseconds => _moveMilliseconds.OnValueChanged;
        private static readonly Slider _moveMilliseconds = new Slider("移動時間[ms]", 10, 1000, 100);
        public static ReactiveProperty<int> EffectDisplayTime => _moveMilliseconds.OnValueChanged;
        private static readonly Slider _effectDisplayTime = new Slider("エフェクト表示時間[ms]", 10, 1000, 100);
        public static ReactiveProperty<bool> IgnoreWall => _ignoreWall.OnValueChanged;
        private static readonly CheckBox _ignoreWall = new CheckBox("壁貫通", false);
        public static List<IOptionInput> GetOptions()
        {
            List<IOptionInput> setters = new List<IOptionInput>();
            foreach (FieldInfo field in typeof(Settings).GetFields(BindingFlags.NonPublic | BindingFlags.Static))
            {
                object value = field.GetValue(typeof(Settings));
                if (typeof(IOptionInput).IsAssignableFrom(field.FieldType))
                {
                    setters.Add((IOptionInput)value);
                }
            }
            return setters;
        }
    }
    public interface IOptionInput { }
    public record Slider : IOptionInput
    {
        public readonly string Name;
        public readonly int Min;
        public readonly int Max;
        public int Value => OnValueChanged.Value;
        public ReactiveProperty<int> OnValueChanged { get; private set; }
        public Slider(string name, int min, int max, int defaultValue)
        {
            Name = name;
            Min = min;
            Max = max;
            OnValueChanged = new ReactiveProperty<int>(defaultValue);
        }
        public void SetValue(int value)
        {
            OnValueChanged.Value = Mathf.Clamp(value, Min, Max);
        }
    }
    public record CheckBox : IOptionInput
    {
        public readonly string Name;
        public bool Value => OnValueChanged.Value;
        public ReactiveProperty<bool> OnValueChanged { get; private set; }
        public CheckBox(string name, bool defaultValue)
        {
            Name = name;
            OnValueChanged = new ReactiveProperty<bool>(defaultValue);
        }
        public void SetValue(bool value)
        {
            OnValueChanged.Value = value;
        }
    }
}
