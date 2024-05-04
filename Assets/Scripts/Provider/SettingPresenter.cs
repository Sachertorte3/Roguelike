using R3;
using Scripts.Model.Setting;
using Scripts.View.UI;
using System;
using Unity.Logging;
using VContainer;

namespace Scripts.Provider
{
    internal class SettingPresenter
    {
        [Inject]
        public SettingPresenter(SettingWindow settingWindow)
        {
            Log.Debug("Set options");
            foreach (IOptionInput option in Settings.GetOptions())
            {
                switch (option)
                {
                    case Slider slider:
                        settingWindow.AddIntOption(slider.Name, slider.Min, slider.Max, slider.Value).Subscribe(value => slider.SetValue(value));
                        break;
                    case CheckBox checkbox:
                        settingWindow.AddBoolOption(checkbox.Name, checkbox.Value).Subscribe(value => checkbox.SetValue(value));
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
        }
    }
}
