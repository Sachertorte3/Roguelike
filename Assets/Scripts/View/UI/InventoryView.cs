#nullable enable
using R3;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    public record Focus(int index, int subIndex, bool isGroundItem, bool isEmpty);
    public class InventoryView : MonoBehaviour
    {
        private const int InventorySize = 10;
        [SerializeField] private InventoryItemView _itemViewPrefab;
        [SerializeField] private TMP_Text _infoText;
        [SerializeField] private Sprite _groundItemIcon;
        [SerializeField] private Sprite _emptyIcon;
        private readonly ReactiveProperty<(int index, int subIndex)> _focusIndex = new((0, -1));
        private readonly Subject<int> _onMainFocusChanged = new();
        private readonly string[] _info = new string[InventorySize + 1];
        private readonly InventoryItemView[] _itemViews = new InventoryItemView[InventorySize + 2];

        public Observable<Focus> OnFocusChanged => _focusIndex.Select(index => GetFocus(index.index, index.subIndex));
        public Observable<Focus> OnMainFocusChanged => _onMainFocusChanged.Select(index => GetFocus(index, -1));
        public Focus CurrentFocus => GetFocus(_focusIndex.CurrentValue.index, _focusIndex.CurrentValue.subIndex);

        private SubStorageView _subStorageView;

        public void Initialize(SubStorageView subStorageView)
        {
            _subStorageView = subStorageView;
            for (var i = 0; i < _itemViews.Length; i++)
                if (_itemViews[i] == null)
                    _itemViews[i] = Instantiate(_itemViewPrefab, transform);
            _itemViews.ForEach((view, index) => view.OnFocus.Subscribe(_ =>
            {
                _onMainFocusChanged.OnNext(index);
                _focusIndex.Value = (index, -1);
            }).AddTo(view));
            subStorageView.OnFocusChanged.Subscribe(subIndex =>
            {
                _focusIndex.Value = (_focusIndex.CurrentValue.index, subIndex);
            }).AddTo(subStorageView);
            OnFocusChanged.Subscribe(index => 
            {
                if (index.subIndex >= 0)
                    _infoText.text = _subStorageView.GetInfo(index.subIndex);
                else
                    _infoText.text = index.isEmpty ? "" : _info[index.index];
            }).AddTo(this);
            OnFocusChanged.Subscribe(index => Debug.Log(index)).AddTo(this);
            _itemViews[InventorySize].SetIcon(_groundItemIcon, null, false, false, true, true);
            _itemViews[InventorySize + 1].SetIcon(_emptyIcon, null, false, false, true, true);

            for (var i = 0; i < _itemViews.Length; i++)
            {
                SetNavigation(i);
            }
        }

        public void Start()
        {
            Select(0);
        }

        public void Select(int index)
        {
            _itemViews[index].Select();
        }

        public void SelectSub(int subIndex)
        {
            _subStorageView.Select(subIndex);
        }

        private Focus GetFocus(int index, int subIndex) => new(index, subIndex, index == InventorySize, index == InventorySize + 1);

        public Selectable Get(int index) => _itemViews[index].GetComponent<Selectable>();

        public void SetNavigationWithSubStorage(SubStorageView subStorageView, int index)
        {
            var nav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = _itemViews[(index - 1 + _itemViews.Length) % _itemViews.Length]
                    .GetComponent<Selectable>(),
                selectOnRight = _itemViews[(index + 1) % _itemViews.Length].GetComponent<Selectable>(),
                selectOnDown = subStorageView.First
            };
            _itemViews[index].GetComponent<Selectable>().navigation = nav;
        }

        public void SetNavigation(int index)
        {
            var nav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = _itemViews[(index - 1 + _itemViews.Length) % _itemViews.Length]
                    .GetComponent<Selectable>(),
                selectOnRight = _itemViews[(index + 1) % _itemViews.Length].GetComponent<Selectable>()
            };
            _itemViews[index].GetComponent<Selectable>().navigation = nav;
        }

        public void Replace(Sprite icon, int? count, bool isCursed, bool isShiny, bool isCountIdentified,
            bool isCurseIdentified, string info, int index)
        {
            _itemViews[index].SetIcon(icon, count, isCursed, isShiny, isCountIdentified, isCurseIdentified);
            _info[index] = info;
            UpdateInfo(info, index);
        }

        public void SetGround()
        {
            _itemViews[InventorySize].SetIcon(_groundItemIcon, null, false, false, true, true);
        }

        public void Remove(int index)
        {
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