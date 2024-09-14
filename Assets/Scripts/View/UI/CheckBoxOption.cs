using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    internal class CheckBoxOption : MonoBehaviour, ISettingOption
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Toggle _toggle;
        public Observable<bool> OnValueChanged => _toggle.onValueChanged.AsObservable();
        public Selectable Selectable => _toggle;

        public void SetData(string itemName, bool value)
        {
            _text.SetText(itemName);
            _toggle.isOn = value;
        }
    }
}