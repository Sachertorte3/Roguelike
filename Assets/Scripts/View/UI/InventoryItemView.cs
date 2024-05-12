#nullable enable
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts.View.UI
{
    [RequireComponent(typeof(Image), typeof(Selectable))]
    internal class InventoryItemView : Selectable, ISelectHandler
    {
        [SerializeField] private Image _icon;
        public Observable<Unit> OnFocus => _onFocus;
        private Subject<Unit> _onFocus = new();
        public void SetIcon(Sprite? icon)
        {
            _icon.sprite = icon;
        }
        public override void OnSelect(BaseEventData eventData)
        {
            _onFocus.OnNext(Unit.Default);
            base.OnSelect(eventData);
        }
    }
}
