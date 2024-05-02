using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.View.UI
{
    internal class CheckBoxText : MonoBehaviour
    {
        [SerializeField] Toggle _checkBox;
        [SerializeField] TMP_Text _text;
        private string GetText(bool value) => value ? "有効" : "無効";
        private void Start()
        {
            _text.SetText(GetText(_checkBox.isOn));
            _checkBox.onValueChanged.AsObservable().Subscribe(value =>
            {
                _text.SetText(GetText(value));
            });
        }
    }
}
