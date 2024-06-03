using System.Collections.Generic;
using System.Reflection;
using R3;

namespace Data.Setting
{
    public static class Settings
    {
        private static readonly Slider _BGMVolume = new("BGM音量", 0, 100, 50);
        private static readonly Slider _SEVolume = new("SE音量", 0, 100, 50);
        private static readonly Slider _moveMilliseconds = new("移動時間[ms]", 1, 1000, 100);
        private static readonly Slider _dashMilliseconds = new("ダッシュ時移動時間[ms]", 1, 1000, 20);
        private static readonly Slider _throwMilliseconds = new("吹き飛ばし時間[ms]", 1, 1000, 50);
        private static readonly Slider _effectDisplayTime = new("エフェクト表示時間[ms]", 10, 1000, 100);
        private static readonly Slider _damageTextDisplayTime = new("ダメージテキスト表示時間[ms]", 10, 3000, 500);
        private static readonly CheckBox _ignoreWall = new("壁貫通", false);
        private static readonly CheckBox _intelligentDash = new("スマートダッシュ", true);
        private static readonly Slider _dashPauseMilliseconds = new("分岐一時停止時間[ms]", 100, 1000, 250);
        public static ReactiveProperty<int> BGMVolume => _BGMVolume.OnValueChanged;
        public static ReactiveProperty<int> SEVolume => _SEVolume.OnValueChanged;
        public static ReactiveProperty<int> MoveMilliseconds => _moveMilliseconds.OnValueChanged;
        public static ReactiveProperty<int> DashMilliseconds => _dashMilliseconds.OnValueChanged;
        public static ReactiveProperty<int> ThrowMilliseconds => _throwMilliseconds.OnValueChanged;
        public static ReactiveProperty<int> EffectDisplayTime => _effectDisplayTime.OnValueChanged;
        public static ReactiveProperty<int> DamageTextDisplayTime => _damageTextDisplayTime.OnValueChanged;
        public static ReactiveProperty<bool> IgnoreWall => _ignoreWall.OnValueChanged;
        public static ReactiveProperty<bool> IntelligentDash => _intelligentDash.OnValueChanged;
        public static ReactiveProperty<int> DashPauseMilliseconds => _dashPauseMilliseconds.OnValueChanged;

        public static List<IOptionInput> GetOptions()
        {
            List<IOptionInput> setters = new();
            foreach (var field in typeof(Settings).GetFields(BindingFlags.NonPublic | BindingFlags.Static))
            {
                var value = field.GetValue(typeof(Settings));
                if (typeof(IOptionInput).IsAssignableFrom(field.FieldType)) setters.Add((IOptionInput)value);
            }

            return setters;
        }
    }
}