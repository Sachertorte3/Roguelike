using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.View.UI
{
    internal class SliderOption : MonoBehaviour
    {
        [SerializeField] TMP_Text _text;
        [SerializeField] Slider _slider;
        public IObservable<int> OnValueChanged => _slider.onValueChanged.AsObservable().Select(value => (int)value);
        public void SetData(string itemName, int min, int max, int value)
        {
            _text.SetText(itemName);
            _slider.minValue = min;
            _slider.maxValue = max;
            _slider.value = value;
        }
    }
}
