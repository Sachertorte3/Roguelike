#nullable enable
using UnityEngine;

namespace Scripts.View.UI
{
    public class InventoryView : MonoBehaviour
    {
        private InventoryItemView[] itemViews = new InventoryItemView[10];
        [SerializeField] private InventoryItemView itemViewPrefab;
        private void Awake()
        {
            for (int i = 0; i < itemViews.Length; i++)
            {
                itemViews[i] = Instantiate(itemViewPrefab, transform);
            }
        }
        public void Replace(Sprite? icon, int index)
        {
            itemViews[index].SetIcon(icon);
        }
    }
}
