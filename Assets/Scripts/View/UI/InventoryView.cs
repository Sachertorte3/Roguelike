#nullable enable
using R3;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    public class InventoryView : MonoBehaviour
    {
        private const int InventorySize = 10;
        [SerializeField] private InventoryItemView _itemViewPrefab;
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private Sprite _groundItemIcon;
        [SerializeField] private Sprite _emptyIcon;
        private readonly ReactiveProperty<int> _focusIndex = new();
        private readonly string[] _info = new string[InventorySize+1];
        private readonly InventoryItemView[] _itemViews = new InventoryItemView[InventorySize + 2];

        public ReadOnlyReactiveProperty<(int index, bool isGroundItem, bool isEmpty)> OnFocusChanged => _focusIndex
            .Select(index => (index, index == InventorySize, index == InventorySize + 1)).ToReadOnlyReactiveProperty();

        public (int index, bool isGroundItem, bool isEmpty) CurrentFocus => OnFocusChanged.CurrentValue;

        private void Awake()
        {
            for (var i = 0; i < _itemViews.Length; i++)
                if (_itemViews[i] == null)
                    _itemViews[i] = Instantiate(_itemViewPrefab, transform);
            _itemViews.ForEach((view, index) => view.OnFocus.Subscribe(_ => _focusIndex.Value = index));
            OnFocusChanged.Subscribe(index => { _infoText.text = index.isEmpty ? "" : _info[index.index]; })
                .AddTo(this);
            _itemViews[0].Select();
            _itemViews[InventorySize].SetIcon(_groundItemIcon, null, false, false, true, true);
            _itemViews[InventorySize + 1].SetIcon(_emptyIcon, null, false, false, true, true);

            for (var i = 0; i < _itemViews.Length; i++)
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

        public void Replace(Sprite icon, int? count, bool isCursed, bool isShiny, bool isCountIdentified, bool isCurseIdentified, string info, int index)
        {
            if (_itemViews[index] == null)
                _itemViews[index] = Instantiate(_itemViewPrefab, transform);
            _itemViews[index].SetIcon(icon, count, isCursed, isShiny, isCountIdentified, isCurseIdentified);
            _info[index] = info;
            UpdateInfo(info, index);
        }

        public void SetGround()
        {
            if (_itemViews[InventorySize] != null)
                _itemViews[InventorySize].SetIcon(_groundItemIcon, null, false, false, true, true);
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
            if (CurrentFocus.index == index) _infoText.text = info;
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