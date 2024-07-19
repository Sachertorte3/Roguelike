#nullable enable
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View.UI
{
    [RequireComponent(typeof(Image), typeof(Selectable))]
    internal class InventoryItemView : Selectable, ISelectHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _text;
        private readonly Subject<Unit> _onFocus = new();
        public Observable<Unit> OnFocus => _onFocus;

        public override void OnSelect(BaseEventData eventData)
        {
            _onFocus.OnNext(Unit.Default);
            base.OnSelect(eventData);
        }

        public void SetIcon(Sprite icon, int? count)
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

        public void SetCount(int? count)
        {
            if (count.HasValue)
                _text.text = count.ToString();
            else
                _text.text = "";
        }

        public void RemoveCount()
        {
            _text.text = "";
        }
    }
}