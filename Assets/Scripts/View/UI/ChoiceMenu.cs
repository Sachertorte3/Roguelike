#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace View.UI
{
    public class ChoiceMenu : MonoBehaviour, IMenu
    {
        public bool CanClose => false;
        private readonly ReactiveProperty<int> _selectedIndex = new(-1);
        public ReadOnlyReactiveProperty<int> SelectedIndex => _selectedIndex;
        private readonly AsyncReactiveProperty<int> _choicedIndex = new(-1);
        public IReadOnlyAsyncReactiveProperty<int> ChoicedIndex => _choicedIndex;
        [SerializeField] private TextMeshProUGUI _choiceText;
        [SerializeField] private RectTransform _content;
        [SerializeField] private ChoiceButton _choiceButtonPrefab;
        private readonly List<ChoiceButton> _buttons = new();

        public void SetChoices(string? choiceText, params string[] choices)
        {
            foreach (var button in _buttons)
            {
                Destroy(button.gameObject);
            }

            _buttons.Clear();

            if (choiceText != null)
                _choiceText.text = choiceText;
            else
                _choiceText.text = "";

            _selectedIndex.Value = 0;
            foreach (var (choice, index) in choices.Index())
            {
                var button = Instantiate(_choiceButtonPrefab, _content);
                button.Construct(choice,
                    () => _selectedIndex.Value = index,
                    () =>
                    {
                        _choicedIndex.Value = index;
                    });
                _buttons.Add(button);
            }

            for (var i = 0; i < _buttons.Count; i++)
            {
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = _buttons[(i - 1 + _buttons.Count) % _buttons.Count].GetComponent<Button>(),
                    selectOnDown = _buttons[(i + 1) % _buttons.Count].GetComponent<Button>()
                };
                _buttons[i].GetComponent<Button>().navigation = nav;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }
    }
}