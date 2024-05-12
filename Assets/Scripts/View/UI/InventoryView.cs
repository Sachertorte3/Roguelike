#nullable enable
using Assets.Scripts.View.UI;
using R3;
using Sirenix.Utilities;
using System.Linq;
using UnityEngine;

namespace Scripts.View.UI
{
    public class InventoryView : MonoBehaviour
    {
        private InventoryItemView[] itemViews = new InventoryItemView[10];
        [SerializeField] private InventoryItemView itemViewPrefab;
        public Observable<int> OnFocusChanged => _onFocusChanged;
        private Subject<int> _onFocusChanged = new Subject<int>();
        private void Awake()
        {
            for (int i = 0; i < itemViews.Length; i++)
            {
                itemViews[i] = Instantiate(itemViewPrefab, transform);
            }
            itemViews.ForEach((view, index) => view.OnFocus.Subscribe(_ => _onFocusChanged.OnNext(index)));
            itemViews[0].Select();
        }
        public void Replace(Sprite icon, int count, int index)
        {
            itemViews[index].SetIcon(icon, count);
        }
        public void Remove(int index)
        {
            itemViews[index].Remove();
        }
        public void UpdateCount(int count, int index)
        {
            itemViews[index].SetCount(count);
        }
    }
}
