using UnityEngine;

namespace View.UI
{
    public class DungeonMenu : MonoBehaviour, IMenu
    {
        [SerializeField] private InventoryView _inventory;
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
            _inventory.EnableAllItems();
        }

        public void Disable()
        {
            _inventory.DisableAllItems();
        }
    }
}