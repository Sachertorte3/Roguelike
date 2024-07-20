using System.Collections.Generic;
using System.Reflection;
using R3;

namespace Domain.Model.Setting
{
    public static class Settings
    {
        private static readonly Slider _BGMVolume = new("BGM音量", 0, 100, 50);
        private static readonly Slider _SEVolume = new("SE音量", 0, 100, 50);
        private static readonly Slider _moveMilliseconds = new("移動時間[ms]", 1, 1000, 100);
        private static readonly Slider _dashMilliseconds = new("ダッシュ時移動時間[ms]", 1, 1000, 20);
        private static readonly Slider _throwMilliseconds = new("吹き飛ばし時間[ms]", 1, 1000, 50);
        private static readonly Slider _effectDisplayTime = new("エフェクト表示時間[ms]", 10, 1000, 100);
        private static readonly Slider _logShownMilliSeconds = new("ログ表示時間[ms]", 10, 10000, 5000);
        private static readonly Slider _damageTextDisplayTime = new("ダメージテキスト表示時間[ms]", 10, 3000, 500);
        private static readonly Slider _flushDuration = new("フラッシュ時間[ms]", 10, 5000, 1000);
        private static readonly Slider _significantDamageThresholdPercentage = new("大ダメージ閾値[%]", 1, 100, 25);
        private static readonly Slider _lowHpThresholdPercentage = new("低HP警告閾値[%]", 1, 100, 25);
        private static readonly CheckBox _ignoreWall = new("壁貫通", false);
        private static readonly CheckBox _intelligentDash = new("スマートダッシュ", true);
        private static readonly CheckBox _autoPickUpShopItem = new("店のアイテムを自動で拾う", true);
        private static readonly Slider _dashPauseMilliseconds = new("分岐一時停止時間[ms]", 100, 1000, 250);

        public static ReactiveProperty<int> BGMVolume => _BGMVolume.OnValueChanged;
        public static ReactiveProperty<int> SEVolume => _SEVolume.OnValueChanged;
        public static ReactiveProperty<int> MoveMilliseconds => _moveMilliseconds.OnValueChanged;
        public static ReactiveProperty<int> DashMilliseconds => _dashMilliseconds.OnValueChanged;
        public static ReactiveProperty<int> ThrowMilliseconds => _throwMilliseconds.OnValueChanged;
        public static ReactiveProperty<int> EffectDisplayTime => _effectDisplayTime.OnValueChanged;
        public static ReactiveProperty<int> LogShownMilliSeconds => _logShownMilliSeconds.OnValueChanged;
        public static ReactiveProperty<int> DamageTextDisplayTime => _damageTextDisplayTime.OnValueChanged;
        public static ReactiveProperty<int> FlushDuration => _flushDuration.OnValueChanged;
        public static ReactiveProperty<int> SignificantDamageThresholdPercentage =>
            _significantDamageThresholdPercentage.OnValueChanged;
        public static ReactiveProperty<int> LowHpThresholdPercentage => _lowHpThresholdPercentage.OnValueChanged;
        public static ReactiveProperty<bool> IgnoreWall => _ignoreWall.OnValueChanged;
        public static ReactiveProperty<bool> IntelligentDash => _intelligentDash.OnValueChanged;
        public static ReactiveProperty<bool> AutoPickUpShopItem => _autoPickUpShopItem.OnValueChanged;
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