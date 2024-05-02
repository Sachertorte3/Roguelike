using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.View.UI
{
    internal class CheckBoxOption : MonoBehaviour
    {
        [SerializeField] TMP_Text _text;
        [SerializeField] Toggle _toggle;
        public IObservable<bool> OnValueChanged => _toggle.onValueChanged.AsObservable();
        public void SetData(string itemName, bool value)
        {
            _text.SetText(itemName);
            _toggle.isOn = value;
        }
    }
}
