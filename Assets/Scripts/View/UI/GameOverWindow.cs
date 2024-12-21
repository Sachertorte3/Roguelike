using TMPro;
using UnityEngine;

namespace View.UI
{
    public class GameOverWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _causeOfDeathText;
        public void SetData(int level, string causeOfDeath)
        {
            _levelText.text = $"到達階層: {level}F";
            _causeOfDeathText.text = causeOfDeath;
        }
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}