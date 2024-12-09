#nullable enable
using System;
using System.Linq;
using R3;
using Sirenix.Utilities;
using Unity.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    public class StorageView : MonoBehaviour
    {
        [SerializeField] private InventoryItemView _itemViewPrefab;
        private readonly Subject<int> _onSelected = new();
        private InventoryItemView[] _itemViews = Array.Empty<InventoryItemView>();
        public int Capacity => _itemViews.Length;
        public Selectable? First => _itemViews.FirstOrDefault()?.GetComponent<Selectable>();
        public Observable<int> OnSelected => _onSelected;
        public void SetCapacity(int capacity)
        {
            Clear();
            _itemViews = new InventoryItemView[capacity];
            for (var i = 0; i < capacity; i++)
            {
                _itemViews[i] = Instantiate(_itemViewPrefab, transform);
            }
            _itemViews.ForEach((view, index) => view.OnSelected.Subscribe(_ => _onSelected.OnNext(index)).AddTo(view));

            for (var i = 0; i < capacity; i++)
            {
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnLeft = _itemViews[(i - 1 + _itemViews.Length) % _itemViews.Length]
                        .GetComponent<Selectable>(),
                    selectOnRight = _itemViews[(i + 1) % _itemViews.Length].GetComponent<Selectable>()
                };
                _itemViews[i].GetComponent<Selectable>().navigation = nav;
            }
        }
        public void SetDefaultIcon(int index, Sprite icon)
        {
            _itemViews[index].SetDefaultIcon(icon);
        }
        public void ResetNavigation()
        {
            for (var i = 0; i < Capacity; i++)
            {
                var nav = _itemViews[i].GetComponent<Selectable>().navigation;
                nav.selectOnUp = null;
                nav.selectOnDown = null;
                _itemViews[i].GetComponent<Selectable>().navigation = nav;
            }
        }
        public void SetParentNavigation(StorageView parent, int index)
        {
            for (var i = 0; i < Capacity; i++)
            {
                var nav = _itemViews[i].GetComponent<Selectable>().navigation;
                nav.selectOnUp = parent._itemViews[index];
                _itemViews[i].GetComponent<Selectable>().navigation = nav;
            }
        }
        public void SetChildrenNavigation(StorageView children)
        {
            for (var i = 0; i < Capacity; i++)
            {
                var nav = _itemViews[i].GetComponent<Selectable>().navigation;
                nav.selectOnDown = children._itemViews.First();
                _itemViews[i].GetComponent<Selectable>().navigation = nav;
            }
        }
        public void Clear()
        {
            foreach (var view in _itemViews)
                Destroy(view.gameObject);
            _itemViews = Array.Empty<InventoryItemView>();
        }

        public void Select(int index)
        {
            _itemViews[index].Select();
        }

        public void Replace(ItemViewData itemViewData, int index, bool interactable)
        {
            _itemViews[index].Set(itemViewData.icon, itemViewData.count, itemViewData.isCursed, itemViewData.isShiny, itemViewData.isCountIdentified, itemViewData.isCurseIdentified);
            _itemViews[index].UpdateInteractable(interactable);
        }

        public void Remove(int index)
        {
            _itemViews[index].Remove();
            _itemViews[index].UpdateInteractable(true);
        }
        public void EnableAll()
        {
            foreach (var view in _itemViews)
                view.UpdateInteractable(true);
        }
        public void DisableAll()
        {
            foreach (var view in _itemViews)
                view.UpdateInteractable(false);
        }
    }
}