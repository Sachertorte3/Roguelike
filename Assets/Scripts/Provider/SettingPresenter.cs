using System;
using Domain.Model.Setting;
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
            SetOptions(settingWindow);
        }

        public void SetOptions(SettingWindow settingWindow)
        {
            settingWindow.Clear();
            foreach (var option in Settings.GetOptions())
                switch (option)
                {
                    case Slider slider:
                        settingWindow.AddIntOption(slider.Name, slider.Min, slider.Max, slider.Value, slider.IsEnabled);
                        break;
                    case CheckBox checkbox:
                        settingWindow.AddBoolOption(checkbox.Name, checkbox.Value, checkbox.IsEnabled);
                        break;
                    default:
                        throw new ArgumentException();
                }
        }
    }
}