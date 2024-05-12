#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.View.UI
{
    [RequireComponent(typeof(Image))]
    internal class InventoryItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        private bool _isFocused = false;
        public void SetIcon(Sprite? icon)
        {
            _icon.sprite = icon;
        }
        public void Focus()
        {
            _isFocused = true;
        }
        public void Unfocus()
        {
            _isFocused = false;
        }
    }
}
