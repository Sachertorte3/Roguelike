using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    public class StatLine : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Image _statBar;

        public void SetValue(float maxValue, float value)
        {
            _text.text = $"{value}/{maxValue}";
            _statBar.fillAmount = value / maxValue;
        }
        public void SetTextColor(Color color)
        {
            _text.color = color;
        }
    }
}