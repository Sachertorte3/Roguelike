#nullable enable
using System;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using R3;

namespace View.UI
{
    public class SubStorageView : MonoBehaviour
    {
        private int _mainIndex;
        [SerializeField] private InventoryItemView _itemViewPrefab;
        private readonly Subject<int> _onFocusChanged = new();
        private readonly Subject<int> _onLogUpdated = new();
        private InventoryItemView[] _itemViews = Array.Empty<InventoryItemView>();
        private string[] _info = Array.Empty<string>();
        public int Capacity => _itemViews.Length;
        public Selectable? First => _itemViews.FirstOrDefault()?.GetComponent<Selectable>();
        public Observable<int> OnFocusChanged => _onFocusChanged;
        public Observable<int> OnLogUpdated => _onLogUpdated;
        public void SetCapacity(Selectable root, int mainIndex, int capacity)
        {
            Clear();
            _mainIndex = mainIndex;
            _itemViews = new InventoryItemView[capacity];
            _info = new string[capacity];
            for (var i = 0; i < capacity; i++)
            {
                _itemViews[i] = Instantiate(_itemViewPrefab, transform);
            }
            _itemViews.ForEach((view, index) => view.OnFocus.Subscribe(_ => _onFocusChanged.OnNext(index)).AddTo(view));

            for (var i = 0; i < capacity; i++)
            {
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnLeft = _itemViews[(i - 1 + _itemViews.Length) % _itemViews.Length]
                        .GetComponent<Selectable>(),
                    selectOnRight = _itemViews[(i + 1) % _itemViews.Length].GetComponent<Selectable>(),
                    selectOnUp = root
                };
                _itemViews[i].GetComponent<Selectable>().navigation = nav;
            }
        }
        public void Clear()
        {
            _mainIndex = -1;
            foreach (var view in _itemViews)
                Destroy(view.gameObject);
            _itemViews = Array.Empty<InventoryItemView>();
        }

        public void Select(int index)
        {
            _itemViews[index].Select();
        }

        public string GetInfo(int index)
        {
            return _info[index];
        }

        public void Replace(Sprite icon, int? count, bool isCursed, bool isShiny, bool isCountIdentified,
            bool isCurseIdentified, string info, int index)
        {
            if (_itemViews[index] == null)
                _itemViews[index] = Instantiate(_itemViewPrefab, transform);
            _itemViews[index].SetIcon(icon, count, isCursed, isShiny, isCountIdentified, isCurseIdentified);
            _info[index] = info;
            UpdateInfo(info, index);
        }

        public void Remove(int index)
        {
            if (_itemViews[index] == null)
                _itemViews[index] = Instantiate(_itemViewPrefab, transform);
            _itemViews[index].Remove();
            UpdateInfo("", index);
        }

        public void UpdateInfo(string info, int index)
        {
            _info[index] = info;
            _onLogUpdated.OnNext(index);
        }

        public void DisableItems(int[] disabledItemIndexes)
        {
            foreach (var index in disabledItemIndexes)
                _itemViews[index].Disable();
        }

        public void EnableAllItems()
        {
            foreach (var view in _itemViews)
                view.Enable();
        }

        public void DisableAllItems()
        {
            foreach (var view in _itemViews)
                view.Disable();
        }
    }
}