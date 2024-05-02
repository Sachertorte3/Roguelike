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
        [SerializeField] SliderItem _sliderItem;
        public IObservable<int> AddValueItem(string itemName, int min, int max, int value)
        {
            _sliderItem.SetData(itemName, min, max, value);
            SliderItem newSliderItem = GameObject.Instantiate(_sliderItem, _content);
            return newSliderItem.OnValueChanged;
        }
    }
}
