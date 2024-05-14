using R3;
using UnityEngine;

namespace View.UI
{
    public class SettingWindow : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private SliderOption _sliderItem;
        [SerializeField] private CheckBoxOption _checkBoxItem;

        public Observable<int> AddIntOption(string itemName, int min, int max, int value)
        {
            _sliderItem.SetData(itemName, min, max, value);
            var option = Instantiate(_sliderItem, _content);
            return option.OnValueChanged;
        }

        public Observable<bool> AddBoolOption(string itemName, bool value)
        {
            _checkBoxItem.SetData(itemName, value);
            var option = Instantiate(_checkBoxItem, _content);
            return option.OnValueChanged;
        }
    }
}