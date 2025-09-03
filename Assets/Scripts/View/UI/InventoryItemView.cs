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
        private Sprite? _defaultIcon;
        [SerializeField] private TMP_Text _count;
        [SerializeField] private Image _cursedIcon;
        private ParticleController _particles => _icon.GetComponent<ParticleController>();
        private readonly Subject<Unit> _onFocus = new();
        public Observable<Unit> OnSelected => _onFocus;
        public bool CanSkip { get; private set; } = false;

        public void SetDefaultIcon(Sprite icon)
        {
            _defaultIcon = icon;
            if (_icon.sprite == null)
                SetIcon(_defaultIcon);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            _onFocus.OnNext(Unit.Default);
            base.OnSelect(eventData);
        }

        public void Set(Sprite icon, int? count, bool isCursed, bool isShiny, bool isCountIdentified,
            bool isCurseIdentified)
        {
            SetIcon(icon);
            SetCount(count, isCountIdentified);
            SetCursed(isCursed, isCurseIdentified);
            SetShiny(isShiny);
        }

        public void Remove()
        {
            SetIcon(_defaultIcon);
            RemoveCount();
            SetCursed(false, true);
            SetShiny(false);
        }

        private void SetIcon(Sprite? icon)
        {
            _icon.sprite = icon;
            _icon.enabled = icon != null;
        }

        private void SetCursed(bool isCursed, bool isIdentified)
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

        private void SetShiny(bool isShiny)
        {
            if (isShiny)
                _particles.Add(ParticleType.ShinyStar);
            else
                _particles.Clear();
        }

        private void SetCount(int? count, bool isIdentified)
        {
            if (!isIdentified)
                _count.text = "?";
            else if (count.HasValue)
                _count.text = count.ToString();
            else
                _count.text = "";
        }

        private void RemoveCount()
        {
            _count.text = "";
        }

        public void UpdateInteractable(bool interactable)
        {
            this.interactable = interactable;
        }

        public void UpdateSkip(bool canSkip)
        {
            CanSkip = canSkip;
        }
    }
}