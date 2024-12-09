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
        const int MainStorageIncludeGroundAndEmpty = MainStorageSize + 2;
        public const int GroundItemIndex = MainStorageSize;
        public const int EmptyIndex = MainStorageSize + 1;
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
            Log.Info($"[InventoryView]Initialize");
            UpdateAllItemView();
            _parent.OnFocusChanged
                .Subscribe(index =>
                {
                    _focusIndex.Value = (index, -1);
                    UpdateChildrenView();
                });
            _children.OnFocusChanged
                .Subscribe(index => _focusIndex.Value = (_focusIndex.Value.index, index));
            Focus.Subscribe(index => UpdateInfoText());
        }
        private ItemViewData? GetItem(InventoryViewIndex index)
        {
            Log.Info($"[InventoryView]GetItem Index: {index}");
            if (_items.TryGetValue(index.Index, out var value))
            {
                if (index.SubIndex == -1)
                    return value
                    .main;
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
            Log.Info($"[InventoryView]Clear");
            _items.Clear();
            UpdateAllItemView();
        }
        public void Replace(InventoryViewIndex index, ItemViewData itemData)
        {
            Log.Info($"[InventoryView]Replace Index: {index}");
            if (index.SubIndex < 0)
            {
                if (!_items.ContainsKey(index.Index))
                    _items[index.Index] = (itemData, new Dictionary<int, ItemViewData>());
                else
                    _items[index.Index] = (itemData, _items[index.Index].sub);
                UpdateMainItemView(index.Index);
            }
            else
            {
                _items[index.Index].sub[index.SubIndex] = itemData;
                if (index.Index == Focus.CurrentValue.Index)
                    UpdateSubItemView(index);
            }
        }
        public void Remove(InventoryViewIndex index)
        {
            Log.Info($"[InventoryView]Remove Index: {index}");
            if (index.SubIndex == -1)
            {
                _items.Remove(index.Index);
                UpdateMainItemView(index.Index);
            }
            else
            {
                _items[index.Index].sub.Remove(index.SubIndex);
                if (index.Index == Focus.CurrentValue.Index)
                    UpdateSubItemView(index);
            }
        }
        private InventoryViewIndex GetFocus(int index, int subIndex) => new(index, subIndex, index == GroundItemIndex, index == EmptyIndex);

        public void UpdateMainItemView(int mainIndex)
        {
            Log.Info($"[InventoryView]UpdateMainItem Index: {mainIndex}");
            var index = new InventoryViewIndex(mainIndex, -1);
            var interactable = !_locked.Contains(index);
            if (_items.TryGetValue(mainIndex, out var data))
            {
                _parent.Replace(data.main, index.Index, interactable);
            }
            else
            {
                _parent.Remove(index.Index);
            }

            if (!_enabled)
            {
                _parent.DisableAll();
            }
            if (mainIndex == Focus.CurrentValue.Index)
            {
                UpdateChildrenView();
            }
            UpdateInfoText();
        }

        public void UpdateSubItemView(InventoryViewIndex index)
        {
            Log.Info($"[InventoryView]UpdateSubItem Index: {index}");
            var interactable = !_locked.Contains(index);
            var item = GetItem(index);
            if (item != null)
            {
                _children.Replace(item, index.SubIndex, interactable);
            }
            else
            {
                _children.Remove(index.SubIndex);
            }

            if (!_enabled)
            {
                _children.DisableAll();
            }
            UpdateInfoText();
        }

        public void UpdateAllItemView()
        {
            Log.Info($"[InventoryView]UpdateAllItemView");
            _parent.SetCapacity(MainStorageIncludeGroundAndEmpty);
            _parent.SetDefaultIcon(GroundItemIndex, _groundItemIcon);
            _parent.SetDefaultIcon(EmptyIndex, _emptyIcon);
            foreach (var (index, data) in _items)
            {
                var interactable = !_locked.Contains(new InventoryViewIndex(index, -1));
                _parent.Replace(data.main, index, interactable);
            }
            if (!_enabled)
            {
                _parent.DisableAll();
            }
            UpdateChildrenView();
            UpdateInfoText();
        }

        public void UpdateChildrenView()
        {
            Log.Info($"[InventoryView]UpdateChildrenView");
            var currentFocus = Focus.CurrentValue;
            _children.SetCapacity(SubStorageSizes(currentFocus.Index));
            if (_items.TryGetValue(currentFocus.Index, out var mainData))
            {
                foreach (var (index, data) in mainData.sub)
                {
                    var interactable = !_locked.Contains(new InventoryViewIndex(currentFocus.Index, index));
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
            if (currentFocus.SubIndex >= 0 && currentFocus.SubIndex < _children.Capacity)
                _children.Select(currentFocus.SubIndex);
            else
                _parent.Select(currentFocus.Index);
            UpdateInfoText();
        }

        private void UpdateInfoText()
        {
            Log.Info($"[InventoryView]UpdateInfoText");
            var currentFocus = Focus.CurrentValue;
            var item = GetItem(currentFocus);
            if (item != null)
                _infoText.text = item.info;
            else
                _infoText.text = "";
        }

        public void LockItems(InventoryViewIndex[] lockedItemIndexes)
        {
            Log.Info($"[InventoryView]LockItems: Count: {lockedItemIndexes.Length}");
            foreach (var focus in lockedItemIndexes)
                _locked.Add(focus);
            UpdateAllItemView();
        }

        public void UnlockAllItems()
        {
            Log.Info($"[InventoryView]UnlockAllItems");
            _locked.Clear();
            UpdateAllItemView();
        }

        public void EnableAllItems()
        {
            Log.Info($"[InventoryView]EnableAllItems");
            _enabled = true;
            UpdateAllItemView();
        }

        public void DisableAllItems()
        {
            Log.Info($"[InventoryView]DisableAllItems");
            _enabled = false;
            UpdateAllItemView();
        }
    }
}