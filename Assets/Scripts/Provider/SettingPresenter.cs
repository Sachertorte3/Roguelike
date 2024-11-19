using System;
using Domain.Model.Setting;
using R3;
using Unity.Logging;
using VContainer;
using View.UI;

namespace Provider
{
    internal class SettingPresenter
    {
        [Inject]
        public SettingPresenter(SettingWindow settingWindow)
        {
            Log.Debug("[Menu]Set options window");
            Settings.OnValuesSet.Subscribe(_ => SetOptions(settingWindow));
            SetOptions(settingWindow);
        }

        public void SetOptions(SettingWindow settingWindow)
        {
            settingWindow.Clear();
            foreach (var option in Settings.GetOptions())
                switch (option)
                {
                    case Slider slider:
                        settingWindow.AddIntOption(slider.Name, slider.Min, slider.Max, slider.Value)
                            .Subscribe(value => slider.SetValue(value));
                        break;
                    case CheckBox checkbox:
                        settingWindow.AddBoolOption(checkbox.Name, checkbox.Value)
                            .Subscribe(value => checkbox.SetValue(value));
                        break;
                    default:
                        throw new ArgumentException();
                }
        }
    }
}