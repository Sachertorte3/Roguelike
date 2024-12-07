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
        private bool _isLocked;
        private bool _enabled;

        public override void OnSelect(BaseEventData eventData)
        {
            _onFocus.OnNext(Unit.Default);
            base.OnSelect(eventData);
        }

        public void SetIcon(Sprite icon, int? count, bool isCursed, bool isShiny, bool isCountIdentified,
            bool isCurseIdentified)
        {
            _icon.sprite = icon;
            _icon.enabled = true;
            SetCount(count, isCountIdentified);
            SetCursed(isCursed, isCurseIdentified);
            SetShiny(isShiny);
        }

        public void Remove()
        {
            _icon.sprite = null;
            _icon.enabled = false;
            RemoveCount();
            SetCursed(false, true);
            SetShiny(false);
        }

        public void SetCursed(bool isCursed, bool isIdentified)
        {
            if (!isIdentified)
            {
                _cursedIcon.enabled = false;
            }
            else
            {
                _cursedIcon.enabled = isCursed;
            }
        }

        public void SetShiny(bool isShiny)
        {
            if (isShiny)
                _particles.Add(ParticleType.ShinyStar);
            else
                _particles.Clear();
        }

        public void SetCount(int? count, bool isIdentified)
        {
            if (!isIdentified)
                _count.text = "?";
            else if (count.HasValue)
                _count.text = count.ToString();
            else
                _count.text = "";
        }

        public void RemoveCount()
        {
            _count.text = "";
        }

        public void Lock()
        {
            _isLocked = true;
            UpdateInteractable();
        }

        public void Unlock()
        {
            _isLocked = false;
            UpdateInteractable();
        }

        public void Disable()
        {
            _enabled = false;
            UpdateInteractable();
        }

        public void Enable()
        {
            _enabled = true;
            UpdateInteractable();
        }

        public void UpdateInteractable()
        {
            interactable = !_isLocked && _enabled;
        }
    }
}