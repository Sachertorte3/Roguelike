using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace View.UI
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private DungeonMenu _dungeonMenu;
        [SerializeField] private SettingMenu _settingMenu;
        [SerializeField] private ChoiceMenu _choiceMenu;
        private readonly Stack<IMenu> _menuStack = new();
        private readonly Dictionary<IMenu, GameObject> _selectedObject = new();

        [Inject]
        public void Construct(InputReceiver inputReceiver)
        {
            _menuStack.Push(_dungeonMenu);
            inputReceiver.OnMenuOpening.Subscribe(_ =>
            {
                AddMenu(_settingMenu);
            });
            inputReceiver.OnMenuClosing.Subscribe(_ =>
            {
                PopMenu();
            });
        }

        public async UniTask<int> GetChoice(string text, params string[] choices)
        {
            _choiceMenu.SetChoices(text, choices);
            await UniTask.NextFrame();
            AddMenu(_choiceMenu);
            var selectedIndex = await _choiceMenu.SelectedIndex.WaitAsync();
            PopMenu();
            return selectedIndex;
        }

        public void PushMenu(IMenu pushedMenu)
        {
            var previousMenu = _menuStack.Peek();
            _selectedObject[previousMenu] = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(_selectedObject.GetValueOrDefault(pushedMenu));
            previousMenu.Hide();
            previousMenu.Disable();
            pushedMenu.Show();
            pushedMenu.Enable();
            _menuStack.Push(pushedMenu);
        }

        public void AddMenu(IMenu addedMenu)
        {
            var previousMenu = _menuStack.Peek();
            _selectedObject[previousMenu] = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(_selectedObject.GetValueOrDefault(addedMenu));
            previousMenu.Disable();
            addedMenu.Show();
            addedMenu.Enable();
            _menuStack.Push(addedMenu);
        }

        public void PopMenu()
        {
            var poppedMenu = _menuStack.Pop();
            var previousMenu = _menuStack.Peek();
            _selectedObject[poppedMenu] = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(_selectedObject.GetValueOrDefault(previousMenu));
            previousMenu.Show();
            previousMenu.Enable();
            poppedMenu.Hide();
            poppedMenu.Disable();
        }
    }
}