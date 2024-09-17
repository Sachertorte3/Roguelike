#nullable enable
using System.Linq;
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
        [SerializeField] private Sprite _emptyIcon;
        private readonly ReactiveProperty<int> _focusIndex = new();
        private readonly string[] _info = new string[InventorySize];
        private readonly InventoryItemView[] _itemViews = new InventoryItemView[InventorySize + 1];
        public ReadOnlyReactiveProperty<int?> OnFocusChanged => _focusIndex.Select(index => index < InventorySize ? (int?)index : null).ToReadOnlyReactiveProperty();
        public int? CurrentFocus => OnFocusChanged.CurrentValue;

        private void Awake()
        {
            for (var i = 0; i < _itemViews.Length; i++)
                if (_itemViews[i] == null)
                    _itemViews[i] = Instantiate(_itemViewPrefab, transform);
            _itemViews.ForEach((view, index) => view.OnFocus.Subscribe(_ => _focusIndex.Value = index));
            OnFocusChanged.Subscribe(index => { _infoText.text = index == null ? "" : _info[index.Value]; }).AddTo(this);
            _itemViews[0].Select();
            _itemViews[InventorySize].SetIcon(_emptyIcon, null, false);

            for (int i = 0; i < _itemViews.Length; i++)
            {
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnLeft = _itemViews[(i - 1 + _itemViews.Length) % _itemViews.Length].GetComponent<Selectable>(),
                    selectOnRight = _itemViews[(i + 1) % _itemViews.Length].GetComponent<Selectable>()
                };
                _itemViews[i].GetComponent<Selectable>().navigation = nav;
            }
        }

        public void Replace(Sprite icon, int? count, bool isShiny, string info, int index)
        {
            if (_itemViews[index] == null)
                _itemViews[index] = Instantiate(_itemViewPrefab, transform);
            _itemViews[index].SetIcon(icon, count, isShiny);
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
            if (CurrentFocus == index) _infoText.text = info;
        }

        public void UpdateCount(int? count, int index)
        {
            _itemViews[index].SetCount(count);
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