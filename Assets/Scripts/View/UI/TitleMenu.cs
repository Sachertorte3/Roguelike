using UnityEngine;

namespace View.UI
{
    public class TitleMenu : MonoBehaviour, IMenu
    {
        public bool CanClose => false;
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }
    }
}