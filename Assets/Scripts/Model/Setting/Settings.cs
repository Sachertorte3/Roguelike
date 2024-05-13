using System.Collections.Generic;
using System.Reflection;
using R3;
using UnityEngine;

namespace Scripts.Model.Setting
{
    public static class Settings
    {
        public static ReactiveProperty<int> BGMVolume => _BGMVolume.OnValueChanged;
        private static readonly Slider _BGMVolume = new("BGM音量", 0, 100, 50);
        public static ReactiveProperty<int> SEVolume => _SEVolume.OnValueChanged;
        private static readonly Slider _SEVolume = new("SE音量", 0, 100, 50);
        public static ReactiveProperty<int> MoveMilliseconds => _moveMilliseconds.OnValueChanged;
        private static readonly Slider _moveMilliseconds = new("移動時間[ms]", 1, 1000, 100);
        public static ReactiveProperty<int> DashMilliseconds => _dashMilliseconds.OnValueChanged;
        private static readonly Slider _dashMilliseconds = new("ダッシュ時移動時間[ms]", 1, 1000, 20);
        public static ReactiveProperty<int> ThrowMilliseconds => _throwMilliseconds.OnValueChanged;
        private static readonly Slider _throwMilliseconds = new("吹き飛ばし時間[ms]", 1, 1000, 50);
        public static ReactiveProperty<int> EffectDisplayTime => _effectDisplayTime.OnValueChanged;
        private static readonly Slider _effectDisplayTime = new("エフェクト表示時間[ms]", 10, 1000, 100);
        public static ReactiveProperty<bool> IgnoreWall => _ignoreWall.OnValueChanged;
        private static readonly CheckBox _ignoreWall = new("壁貫通", false);
        public static ReactiveProperty<bool> IntelligentDash => _intelligentDash.OnValueChanged;
        private static readonly CheckBox _intelligentDash = new("スマートダッシュ", true);
        public static ReactiveProperty<int> DashPauseMilliseconds => _dashPauseMilliseconds.OnValueChanged;
        private static readonly Slider _dashPauseMilliseconds = new("分岐一時停止時間[ms]", 100, 1000, 250);
        public static List<IOptionInput> GetOptions()
        {
            List<IOptionInput> setters = new();
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
