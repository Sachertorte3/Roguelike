using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    internal class SliderOption : MonoBehaviour, ISettingOption
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Slider _slider;
        public Observable<int> OnValueChanged => _slider.onValueChanged.AsObservable().Select(value => (int)value);
        public Selectable Selectable => _slider;

        public void SetData(string itemName, int min, int max, int value)
        {
            _text.SetText(itemName);
            _slider.minValue = min;
            _slider.maxValue = max;
            _slider.value = value;
        }
    }
}