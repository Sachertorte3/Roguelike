using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Scripts.View.UI
{
    public class SettingWindow: MonoBehaviour
    {
        [SerializeField] Transform _content;
        [SerializeField] SliderOption _sliderItem;
        [SerializeField] CheckBoxOption _checkBoxItem;
        public IObservable<int> AddIntOption(string itemName, int min, int max, int value)
        {
            _sliderItem.SetData(itemName, min, max, value);
            var option = GameObject.Instantiate(_sliderItem, _content);
            return option.OnValueChanged;
        }
        public IObservable<bool> AddBoolOption(string itemName, bool value)
        {
            _checkBoxItem.SetData(itemName, value);
            var option = GameObject.Instantiate(_checkBoxItem, _content);
            return option.OnValueChanged;
        }
    }
}
