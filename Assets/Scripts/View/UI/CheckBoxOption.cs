using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.View.UI
{
    internal class CheckBoxOption : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Toggle _toggle;
        public Observable<bool> OnValueChanged => _toggle.onValueChanged.AsObservable();
        public void SetData(string itemName, bool value)
        {
            _text.SetText(itemName);
            _toggle.isOn = value;
        }
    }
}
