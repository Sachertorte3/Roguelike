using TMPro;
using UnityEngine;

namespace View.UI
{
    public class GameOverWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _causeOfDeathText;
        public void SetCauseOfDeath(string causeOfDeath)
        {
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