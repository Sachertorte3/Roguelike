#nullable enable
using System;
using System.Linq;
using R3;
using Sirenix.Utilities;
using Unity.Logging;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

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
            Log.Debug($"[View]StorageView SetCapacity: {capacity}");
            Clear();
            _itemViews = new InventoryItemView[capacity];
            for (var i = 0; i < capacity; i++)
            {
                _itemViews[i] = Instantiate(_itemViewPrefab, transform);
            }
            _itemViews.ForEach((view, index) => view.OnSelected.Subscribe(_ => _onSelected.OnNext(index)).AddTo(view));

            UpdateHorizontalNavigation();
        }
        public void SetDefaultIcon(int index, Sprite icon)
        {
            _itemViews[index].SetDefaultIcon(icon);
        }

        private void UpdateHorizontalNavigation()
        {
            Log.Verbose($"[View]StorageView UpdateNavigation");
            for (var i = 0; i < _itemViews.Length; i++)
            {
                var currentNav = _itemViews[i].GetComponent<Selectable>().navigation;
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnLeft = FindInteractableItem(i, -1),
                    selectOnRight = FindInteractableItem(i, 1),
                    selectOnUp = currentNav.selectOnUp,
                    selectOnDown = currentNav.selectOnDown
                };
                _itemViews[i].GetComponent<Selectable>().navigation = nav;
            }
        }

        private Selectable? FindInteractableItem(int currentIndex, int direction)
        {
            var startIndex = currentIndex;
            var index = (currentIndex + direction + _itemViews.Length) % _itemViews.Length;

            while (index != startIndex)
            {
                var canSkip = _itemViews[index].CanSkip;

                if (!canSkip)
                {
                    return _itemViews[index].GetComponent<Selectable>();
                }
                index = (index + direction + _itemViews.Length) % _itemViews.Length;
            }

            return null;
        }
        public void ResetVerticalNavigation()
        {
            Log.Verbose($"[View]StorageView ResetNavigation");
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
            Log.Verbose($"[View]StorageView SetParentNavigation: {index}");
            for (var i = 0; i < Capacity; i++)
            {
                var nav = _itemViews[i].GetComponent<Selectable>().navigation;
                nav.selectOnUp = parent._itemViews[index];
                _itemViews[i].GetComponent<Selectable>().navigation = nav;
            }
        }
        public void SetChildrenNavigation(StorageView children)
        {
            Log.Verbose($"[View]StorageView SetChildrenNavigation");
            for (var i = 0; i < Capacity; i++)
            {
                var nav = _itemViews[i].GetComponent<Selectable>().navigation;
                nav.selectOnDown = children._itemViews.First();
                _itemViews[i].GetComponent<Selectable>().navigation = nav;
            }
        }
        public void Clear()
        {
            Log.Debug($"[View]StorageView Clear (Capacity: {_itemViews.Length})");
            foreach (var view in _itemViews.WhereNotNull())
                Destroy(view.gameObject);
            _itemViews = Array.Empty<InventoryItemView>();
        }

        public void Select(int index)
        {
            Log.Verbose($"[View]StorageView Select: {index}");
            if (_itemViews[index].interactable)
                _itemViews[index].Select();
        }

        public void Replace(ItemViewData itemViewData, int index, bool interactable, bool canSkip)
        {
            Log.Verbose($"[View]StorageView Replace: {index}");
            _itemViews[index].Set(itemViewData.icon, itemViewData.count, itemViewData.isCursed, itemViewData.isShiny, itemViewData.isCountIdentified, itemViewData.isCurseIdentified);
            _itemViews[index].UpdateInteractable(interactable);
            _itemViews[index].UpdateSkip(canSkip);
            UpdateHorizontalNavigation();
        }

        public void Remove(int index, bool interactable, bool canSkip)
        {
            Log.Verbose($"[View]StorageView Remove: {index}");
            _itemViews[index].Remove();
            _itemViews[index].UpdateInteractable(interactable);
            _itemViews[index].UpdateSkip(canSkip);
            UpdateHorizontalNavigation();
        }
        public void EnableAll()
        {
            Log.Debug($"[View]StorageView EnableAll");
            foreach (var view in _itemViews)
                view.UpdateInteractable(true);
            UpdateHorizontalNavigation();
        }
        public void DisableAll()
        {
            Log.Debug($"[View]StorageView DisableAll");
            foreach (var view in _itemViews)
                view.UpdateInteractable(false);
            UpdateHorizontalNavigation();
        }
    }
}