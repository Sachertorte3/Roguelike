#nullable enable
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities;

namespace View.UI
{
    [RequireComponent(typeof(Image), typeof(Selectable))]
    internal class InventoryItemView : Selectable, ISelectHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _count;
        [SerializeField] private Image _cursedIcon;
        private ParticleController _particles => _icon.GetComponent<ParticleController>();
        private readonly Subject<Unit> _onFocus = new();
        public Observable<Unit> OnFocus => _onFocus;

        public override void OnSelect(BaseEventData eventData)
        {
            _onFocus.OnNext(Unit.Default);
            base.OnSelect(eventData);
        }

        public void SetIcon(Sprite icon, int? count, bool isCursed, bool isShiny)
        {
            _icon.sprite = icon;
            _icon.enabled = true;
            SetCount(count);
            SetCursed(isCursed);
            SetShiny(isShiny);
        }

        public void Remove()
        {
            _icon.sprite = null;
            _icon.enabled = false;
            RemoveCount();
            SetCursed(false);
            SetShiny(false);
        }

        public void SetCursed(bool isCursed)
        {
            _cursedIcon.enabled = isCursed;
        }

        public void SetShiny(bool isShiny)
        {
            if (isShiny)
                _particles.Add(ParticleType.ShinyStar);
            else
                _particles.Clear();
        }

        public void SetCount(int? count)
        {
            if (count.HasValue)
                _count.text = count.ToString();
            else
                _count.text = "";
        }

        public void RemoveCount()
        {
            _count.text = "";
        }

        public void Disable()
        {
            _icon.color = Color.gray;
            _cursedIcon.color = Color.gray;
            interactable = false;
        }

        public void Enable()
        {
            _icon.color = Color.white;
            _cursedIcon.color = Color.white;
            interactable = true;
        }
    }
}