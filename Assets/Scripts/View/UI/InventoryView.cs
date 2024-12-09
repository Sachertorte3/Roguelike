#nullable enable
using System.Collections.Generic;
using System.Linq;
using R3;
using TMPro;
using Unity.Logging;
using UnityEngine;

namespace View.UI
{
    public class InventoryView : MonoBehaviour
    {
        const int MainStorageSize = 10;
        const int MainStorageItemSize = MainStorageSize + 1;
        const int MainStorageIncludeGroundAndEmpty = MainStorageSize + 2;
        const int GroundItemIndex = MainStorageSize;
        const int EmptyIndex = MainStorageSize + 1;
        public int SubStorageSizes(int index)
        {
            return _items.TryGetValue(index, out var value) ? value.main.storageSize : 0;
        }
        [SerializeField] private StorageView _parent;
        [SerializeField] private StorageView _children;
        [SerializeField] private InventoryItemView _itemViewPrefab;
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private Sprite _groundItemIcon;
        [SerializeField] private Sprite _emptyIcon;
        private readonly ReactiveProperty<(int index, int subIndex)> _focusIndex = new((0, -1));
        private Dictionary<int, (ItemViewData main, Dictionary<int, ItemViewData> sub)> _items = new();
        private HashSet<InventoryViewIndex> _locked = new();
        private bool _enabled = true;
        private readonly Subject<InventoryViewIndex> _onLogUpdated = new();
        public ReadOnlyReactiveProperty<InventoryViewIndex> Focus => _focusIndex.Select(index => GetFocus(index.index, index.subIndex)).ToReadOnlyReactiveProperty();
        public void Initialize()
        {
            _parent.OnFocusChanged
                .Subscribe(index =>
                {
                    _focusIndex.Value = (index, -1);
                    UpdateChildren();
                });
            _children.OnFocusChanged
                .Subscribe(index => _focusIndex.Value = (_focusIndex.Value.index, index));
        }
        private ItemViewData? GetItem(InventoryViewIndex index)
        {
            if (_items.TryGetValue(index.Index, out var value))
            {
                if (index.SubIndex == -1)
                    return value.main;
                else
                {
                    if (value.sub.TryGetValue(index.SubIndex, out var subValue))
                        return subValue;
                    else
                        return null;
                }
            }
            else
                return null;
        }
        public void Clear()
        {
            _items.Clear();
            UpdateItems();
        }
        public void Replace(InventoryViewIndex index, ItemViewData itemData)
        {
            if (index.SubIndex == -1)
            {
                if (!_items.ContainsKey(index.Index))
                    _items[index.Index] = (itemData, new Dictionary<int, ItemViewData>());
                else
                    _items[index.Index] = (itemData, _items[index.Index].sub);
                UpdateItems();
            }
            else
            {
                _items[index.Index].sub[index.SubIndex] = itemData;
                UpdateItems();
            }
        }
        public void Remove(InventoryViewIndex focus)
        {
            if (focus.SubIndex == -1)
                _items.Remove(focus.Index);
            else
                _items[focus.Index].sub.Remove(focus.SubIndex);
            UpdateItems();
        }
        private InventoryViewIndex GetFocus(int index, int subIndex) => new(index, subIndex, index == GroundItemIndex, index == EmptyIndex);

        public void UpdateItems()
        {
            Debug.Log("UpdateItems");
            Debug.Log($"Focus: {Focus.CurrentValue}");
            _parent.SetCapacity(MainStorageIncludeGroundAndEmpty);
            foreach (var (index, data) in _items)
            {
                var interactable = !_locked.Contains(new InventoryViewIndex(index, -1));
                _parent.Replace(data.main, index, interactable);
            }
            if (!_enabled)
            {
                _parent.DisableAll();
            }
            UpdateChildren();
        }

        public void UpdateChildren()
        {
            var currentFocus = Focus.CurrentValue;
            _children.SetCapacity(SubStorageSizes(currentFocus.Index));
            if (_items.TryGetValue(currentFocus.Index, out var mainData))
            {
                foreach (var (index, data) in mainData.sub)
                {
                    var interactable = !_locked.Contains(new InventoryViewIndex(index, -1));
                    _children.Replace(data, index, interactable);
                }
            }
            if (!_enabled)
            {
                _children.DisableAll();
            }
            if (SubStorageSizes(currentFocus.Index) > 0)
            {
                _parent.SetChildrenNavigation(_children);
                _children.SetParentNavigation(_parent, currentFocus.Index);
            }
            else
            {
                _parent.ResetNavigation();
            }
            Debug.Log($"Selected: {Focus.CurrentValue}");
            if (currentFocus.SubIndex >= 0 && currentFocus.SubIndex < _children.Capacity)
                _children.Select(currentFocus.SubIndex);
            else
                _parent.Select(currentFocus.Index);
        }

        public void LockItems(InventoryViewIndex[] lockedItemIndexes)
        {
            foreach (var focus in lockedItemIndexes)
                _locked.Add(focus);
            UpdateItems();
        }

        public void UnlockAllItems()
        {
            _locked.Clear();
            UpdateItems();
        }

        public void EnableAllItems()
        {
            _enabled = true;
            UpdateItems();
        }

        public void DisableAllItems()
        {
            _enabled = false;
            UpdateItems();
        }
    }
}