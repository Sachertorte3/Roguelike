using R3;
using UnityEngine;

namespace View.UI
{
    public class SettingWindow : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private SliderOption _sliderItem;
        [SerializeField] private CheckBoxOption _checkBoxItem;

        private ISettingOption firstOption;
        private ISettingOption lastOption;

        public Observable<int> AddIntOption(string itemName, int min, int max, int value)
        {
            _sliderItem.SetData(itemName, min, max, value);
            var option = Instantiate(_sliderItem, _content);
            UpdateNavigation(option);
            return option.OnValueChanged;
        }

        public Observable<bool> AddBoolOption(string itemName, bool value)
        {
            _checkBoxItem.SetData(itemName, value);
            var option = Instantiate(_checkBoxItem, _content);
            UpdateNavigation(option);
            return option.OnValueChanged;
        }

        public void Clear()
        {
            foreach (Transform child in _content)
                Destroy(child.gameObject);
            firstOption = null;
            lastOption = null;
        }

        private void UpdateNavigation(ISettingOption newSelectable)
        {
            if (firstOption == null)
            {
                firstOption = newSelectable;
                lastOption = newSelectable;
            }
            else
            {
                var lastNav = lastOption.Selectable.navigation;
                lastNav.selectOnDown = newSelectable.Selectable;
                lastOption.Selectable.navigation = lastNav;

                var firstNav = firstOption.Selectable.navigation;
                firstNav.selectOnUp = newSelectable.Selectable;
                firstOption.Selectable.navigation = firstNav;

                var newNav = newSelectable.Selectable.navigation;
                newNav.selectOnUp = lastOption.Selectable;
                newNav.selectOnDown = firstOption.Selectable;
                newSelectable.Selectable.navigation = newNav;

                lastOption = newSelectable;
            }
        }
    }
}