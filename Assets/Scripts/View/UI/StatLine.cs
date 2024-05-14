using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    public class StatLine: MonoBehaviour
    {
        [SerializeField] TMP_Text _text;
        [SerializeField] Image _statBar;
        public void SetValue(float maxValue, float value)
        {
            _text.text = $"{value}/{maxValue}";
            _statBar.fillAmount = value / maxValue;
        }
    }
}
