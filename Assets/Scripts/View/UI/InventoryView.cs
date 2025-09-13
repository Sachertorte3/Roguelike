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
        public const int MainStorageSize = 10;
        public const int MainStorageIncludeGroundAndEmpty = MainStorageSize + 2;
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
            Log.Debug($"[View]InventoryView Initialize");
            CreateMainView();
            CreateChildrenView(Focus.CurrentValue.Index);
            _items.Clear();
            _locked.Clear();
            UpdateInfoText();

            _parent.OnSelected
                .Subscribe(index => _focusIndex.Value = (index, -1));
            _children.OnSelected
                .Subscribe(index => _focusIndex.Value = (_focusIndex.Value.index, index));
            Focus.Pairwise().Subscribe(index =>
            {
                if (index.Previous.Index != index.Current.Index)
                    UpdateChildrenView();

                UpdateInfoText();
            });
        }
        private ItemViewData? GetItem(InventoryViewIndex index)
        {
            Log.Debug($"[View]InventoryView GetItem Index: {index}");
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
        public void Replace(InventoryViewIndex index, ItemViewData itemData)
        {
            Log.Debug($"[View]InventoryView Replace Index: {index}");
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
            Log.Debug($"[View]InventoryView Remove Index: {index}");
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
            Log.Debug($"[View]InventoryView UpdateMainItem Index: {mainIndex}");
            var index = new InventoryViewIndex(mainIndex, -1);
            var item = GetItem(index);
            var interactable = !_locked.Contains(index);
            var canSkip = !interactable && (item == null || item.storageSize == 0);
            if (item != null)
                _parent.Replace(item, index.Index, interactable, canSkip);
            else
                _parent.Remove(index.Index, interactable, canSkip);

            if (!_enabled)
            {
                _parent.DisableAll();
            }
            if (mainIndex == Focus.CurrentValue.Index)
            {
                UpdateChildrenView();
                if (Focus.CurrentValue.SubIndex < 0)
                    UpdateInfoText();
            }
        }

        public void UpdateSubItemView(InventoryViewIndex index)
        {
            Log.Debug($"[View]InventoryView UpdateSubItem Index: {index}");
            var item = GetItem(index);
            var interactable = !_locked.Contains(index);
            var canSkip = !interactable && (item == null || item.storageSize == 0);
            if (item != null)
                _children.Replace(item, index.SubIndex, interactable, canSkip);
            else
                _children.Remove(index.SubIndex, interactable, canSkip);

            if (!_enabled)
            {
                _children.DisableAll();
            }
            UpdateInfoText();
        }

        public void CreateMainView()
        {
            _parent.SetCapacity(MainStorageIncludeGroundAndEmpty);
            _parent.SetDefaultIcon(GroundItemIndex, _groundItemIcon);
            _parent.SetDefaultIcon(EmptyIndex, _emptyIcon);
        }

        public void UpdateAllItemView()
        {
            Log.Debug($"[View]InventoryView UpdateAllItemView");
            for (var i = 0; i < MainStorageIncludeGroundAndEmpty; i++)
            {
                var index = new InventoryViewIndex(i, -1);
                var item = GetItem(index);
                var interactable = !_locked.Contains(index);
                var canSkip = !interactable && (item == null || item.storageSize == 0);
                if (item != null)
                    _parent.Replace(item, index.Index, interactable, canSkip);
                else
                    _parent.Remove(index.Index, interactable, canSkip);
            }
            if (!_enabled)
            {
                _parent.DisableAll();
            }
            UpdateChildrenView();
            if (Focus.CurrentValue.SubIndex < 0)
                UpdateInfoText();
        }

        public void CreateChildrenView(int index)
        {
            _children.SetCapacity(SubStorageSizes(index));
        }

        public void UpdateChildrenView()
        {
            Log.Debug($"[View]InventoryView UpdateChildrenView");
            var currentFocus = Focus.CurrentValue;
            CreateChildrenView(currentFocus.Index);
            for (var i = 0; i < SubStorageSizes(currentFocus.Index); i++)
            {
                var index = new InventoryViewIndex(currentFocus.Index, i);
                var item = GetItem(index);
                var interactable = !_locked.Contains(index);
                var canSkip = !interactable && (item == null || item.storageSize == 0);
                if (item != null)
                    _children.Replace(item, index.SubIndex, interactable, canSkip);
                else
                    _children.Remove(index.SubIndex, interactable, canSkip);
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
                _parent.ResetVerticalNavigation();
            }
            if (currentFocus.SubIndex >= 0 && currentFocus.SubIndex < _children.Capacity)
                _children.Select(currentFocus.SubIndex);
            else
                _parent.Select(currentFocus.Index);

            if (currentFocus.SubIndex >= 0)
                UpdateInfoText();
        }

        private void UpdateInfoText()
        {
            Log.Debug($"[View]InventoryView UpdateInfoText");
            var currentFocus = Focus.CurrentValue;
            var item = GetItem(currentFocus);
            if (item != null)
                _infoText.text = item.info;
            else
                _infoText.text = "";
        }

        public void LockItems(InventoryViewIndex[] lockedItemIndexes)
        {
            Log.Debug($"[View]InventoryView LockItems: Count: {lockedItemIndexes.Length}");
            foreach (var focus in lockedItemIndexes)
                _locked.Add(focus);
            UpdateAllItemView();
        }

        public void UnlockAllItems()
        {
            Log.Debug($"[View]InventoryView UnlockAllItems");
            _locked.Clear();
            UpdateAllItemView();
        }

        public void EnableAllItems()
        {
            Log.Debug($"[View]InventoryView EnableAllItems");
            if (_enabled)
                return;
            _enabled = true;
            UpdateAllItemView();
        }

        public void DisableAllItems()
        {
            Log.Debug($"[View]InventoryView DisableAllItems");
            if (!_enabled)
                return;
            _enabled = false;
            UpdateAllItemView();
        }
    }
}