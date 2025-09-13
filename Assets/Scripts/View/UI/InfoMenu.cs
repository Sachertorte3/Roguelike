#nullable enable
using TMPro;
using UnityEngine;

namespace View.UI
{
    public class InfoMenu : MonoBehaviour, IMenu
    {
        public bool CanClose => false;
        [SerializeField] private TextMeshProUGUI _infoTitleText;
        [SerializeField] private TextMeshProUGUI _infoText;

        public void SetInfo(string infoTitle, string info)
        {
            _infoTitleText.text = infoTitle;
            _infoText.text = info;
        }

        public void Show()
        {
            Debug.Log($"[InfoMenu]Show:");
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            Debug.Log($"[InfoMenu]Hide:");
            gameObject.SetActive(false);
        }

        public void Enable()
        {
            Debug.Log($"[InfoMenu]Enable:");
        }

        public void Disable()
        {
            Debug.Log($"[InfoMenu]Disable:");
        }
    }
}