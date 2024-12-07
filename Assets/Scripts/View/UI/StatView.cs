using TMPro;
using UnityEngine;

namespace View.UI
{
    public class StatView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _level;
        [SerializeField] private StatLine _hp;
        public void SetLevel(int level)
        {
            _level.text = $"Lv.{level}";
        }
        public void SetHp(float maxValue, float value)
        {
            _hp.SetValue(maxValue, value);
        }
        public void SetTextColor(Color color)
        {
            _hp.SetTextColor(color);
        }
    }
}