using UnityEngine;

namespace View.UI
{
    public class TitleMenu : MonoBehaviour, IMenu
    {
        [SerializeField] private GameOverWindow _gameOverPanel;
        public bool CanClose => false;
        public void SetCauseOfDeath(string causeOfDeath)
        {
            _gameOverPanel.SetCauseOfDeath(causeOfDeath);
            _gameOverPanel.Show();
        }
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _gameOverPanel.Hide();
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }
    }
}