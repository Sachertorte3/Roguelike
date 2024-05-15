#nullable enable
using R3;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace View.UI
{
    public class InventoryView : MonoBehaviour
    {
        [SerializeField] private InventoryItemView _itemViewPrefab;
        [SerializeField] private TMP_Text _infoText;
        private readonly ReactiveProperty<int> _focusIndex = new();
        private readonly string[] _info = new string[10];
        private readonly InventoryItemView[] _itemViews = new InventoryItemView[10];
        public int CurrentFocus => _focusIndex.CurrentValue;
        public Observable<int> OnFocusChanged => _focusIndex;

        private void Awake()
        {
            for (var i = 0; i < _itemViews.Length; i++) _itemViews[i] = Instantiate(_itemViewPrefab, transform);
            _itemViews.ForEach((view, index) => view.OnFocus.Subscribe(_ => _focusIndex.Value = index));
            OnFocusChanged.Subscribe(index => { _infoText.text = _info[index]; }).AddTo(this);
            _itemViews[0].Select();
        }

        public void Replace(Sprite icon, int count, string info, int index)
        {
            _itemViews[index].SetIcon(icon, count);
            _info[index] = info;
            UpdateInfo(info, index);
        }

        public void Remove(int index)
        {
            _itemViews[index].Remove();
            UpdateInfo("", index);
        }

        private void UpdateInfo(string info, int index)
        {
            _info[index] = info;
            if (CurrentFocus == index) _infoText.text = info;
        }

        public void UpdateCount(int count, int index)
        {
            _itemViews[index].SetCount(count);
        }
    }
}