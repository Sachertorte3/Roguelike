using System;
using System.Collections.Generic;
using System.Reflection;
using R3;

namespace Domain.Model.Setting
{
    public static class Settings
    {
        public static readonly Slider BGMVolume = new("BGM音量", 0, 100, 50);
        public static readonly Slider SEVolume = new("SE音量", 0, 100, 50);
        public static readonly Slider MoveMilliseconds = new("移動時間[ms]", 1, 1000, 150);
        public static readonly Slider DashMilliseconds = new("ダッシュ時移動時間[ms]", 1, 1000, 20);
        public static readonly Slider ThrowMilliseconds = new("吹き飛ばし時間[ms]", 1, 1000, 50);
        public static readonly Slider EffectDisplayTime = new("エフェクト表示時間[ms]", 10, 1000, 100);
        public static readonly Slider LogShownMilliSeconds = new("ログ表示時間[ms]", 10, 10000, 5000);
        public static readonly Slider DamageTextDisplayTime = new("ダメージテキスト表示時間[ms]", 10, 3000, 500);
        public static readonly Slider FlushDuration = new("フラッシュ時間[ms]", 10, 5000, 1000);
        public static readonly Slider SignificantDamageThresholdPercentage = new("大ダメージ閾値[%]", 1, 100, 25);
        public static readonly Slider LowHpThresholdPercentage = new("低HP警告閾値[%]", 1, 100, 25);
        public static readonly CheckBox IntelligentDash = new("スマートダッシュ", true);
        public static readonly CheckBox AutoPickUpShopItem = new("店のアイテムを自動で拾う", false);
        public static readonly Slider DashPauseMilliseconds = new("分岐一時停止時間[ms]", 100, 1000, 250);
        public static readonly CheckBox AutoSave = new("自動でセーブする", false);
        public static readonly CheckBox EnableCheat = new("チートを有効にする", false);
        public static readonly CheckBox RetryOnDead = new("死亡時にリトライ可能", false, EnableCheat.Value);
        public static readonly CheckBox AutoIdentify = new("全てのアイテムが自動で識別される", false, EnableCheat.Value);
        private static readonly Subject<Unit> _onValuesSet = new();
        public static Observable<Unit> OnValuesSet => _onValuesSet;

        public static List<IOptionInput> GetOptions()
        {
            List<IOptionInput> setters = new();
            foreach (var field in typeof(Settings).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var value = field.GetValue(typeof(Settings));
                if (typeof(IOptionInput).IsAssignableFrom(field.FieldType))
                {
                    setters.Add((IOptionInput)value);
                }
            }

            return setters;
        }

        public static void SetValues(Dictionary<string, int> values)
        {
            foreach (var field in typeof(Settings).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var value = field.GetValue(typeof(Settings));
                if (values.TryGetValue(field.Name, out var intValue))
                {

                    switch (value)
                    {
                        case Slider slider:
                            slider.Value.Value = intValue;
                            break;
                        case CheckBox checkBox:
                            checkBox.Value.Value = intValue == 1;
                            break;
                    }
                }
            }
            _onValuesSet.OnNext(Unit.Default);
        }

        public static Dictionary<string, int> GetValues()
        {
            var settings = new Dictionary<string, int>();
            foreach (var field in typeof(Settings).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var value = field.GetValue(typeof(Settings));
                if (typeof(IOptionInput).IsAssignableFrom(field.FieldType))
                {
                    var option = (IOptionInput)value switch
                    {
                        Slider slider => slider.CurrentValue,
                        CheckBox checkBox => checkBox.CurrentValue ? 1 : 0,
                        _ => throw new InvalidOperationException(""),
                    };
                    settings.Add(field.Name, option);
                }
            }
            return settings;
        }
    }
}