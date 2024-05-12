#nullable enable
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts.View.UI
{
    [RequireComponent(typeof(Image), typeof(Selectable))]
    internal class InventoryItemView : Selectable, ISelectHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _text;
        public Observable<Unit> OnFocus => _onFocus;
        private Subject<Unit> _onFocus = new();
        public void SetIcon(Sprite icon, int count)
        {
            _icon.sprite = icon;
            _icon.enabled = true;
            SetCount(count);
        }
        public void Remove()
        {
            _icon.sprite = null;
            _icon.enabled = false;
            RemoveCount();
        }
        public void SetCount(int count)
        {
            _text.text = count.ToString();
        }
        public void RemoveCount()
        {
            _text.text = "";
        }
        public override void OnSelect(BaseEventData eventData)
        {
            _onFocus.OnNext(Unit.Default);
            base.OnSelect(eventData);
        }
    }
}
