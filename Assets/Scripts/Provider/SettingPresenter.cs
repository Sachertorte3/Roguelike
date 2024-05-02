using Scripts.Model.Setting;
using Scripts.View.UI;
using System;
using UniRx;
using Unity.Logging;
using VContainer;
using VContainer.Unity;

namespace Scripts.Provider
{
    internal class SettingPresenter: IInitializable
    {
        [Inject]
        public SettingPresenter(SettingWindow settingWindow)
        {
            Log.Debug("Set options");
            foreach (var option in Settings.GetOptions())
            {
                switch (option)
                {
                    case Slider slider:
                        settingWindow.AddValueItem(slider.Name, slider.Min, slider.Max, slider.Value).Subscribe(value => slider.SetValue(value));
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
        }
        public void Initialize()
        {

        }
    }
}
