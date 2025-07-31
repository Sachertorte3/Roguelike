#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ObservableCollections;
using R3;
using Unity.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace View.UI
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private TitleMenu _titleMenu;
        [SerializeField] private DungeonMenu _dungeonMenu;
        [SerializeField] private SettingWindow _settingMenu;
        [SerializeField] private ItemLibraryView _itemLibraryMenu;
        [SerializeField] private ChoiceMenu _choiceMenu;
        [SerializeField] private TextInputMenu _textInputMenu;
        [SerializeField] private MainMenu _mainMenu;
        private readonly ObservableStack<IMenu> _menuStack = new();
        private readonly Dictionary<IMenu, GameObject> _selectedObject = new();
        private MenuType _currentMenuType = MenuType.Field;

        private enum MenuType
        {
            Field,
            Menu,
        }

        [Inject]
        public void Construct(InputReceiver inputReceiver)
        {
            inputReceiver.OnMainMenuOpening.Subscribe(_ =>
            {
                if (!_menuStack.Contains(_mainMenu))
                {
                    AddMenu(_mainMenu);
                }
            });
            inputReceiver.OnMenuClosing.Subscribe(_ =>
            {
                if (_menuStack.Count > 0 && _menuStack.Peek().CanClose)
                {
                    PopMenu();
                }
            });
            _menuStack.ObserveChanged().Subscribe(_ =>
            {
                var menuType = GetCurrentMenuType();
                if (!menuType.HasValue)
                    return;
                if (menuType == _currentMenuType)
                    return;
                _currentMenuType = menuType.Value;
                switch (_currentMenuType)
                {
                    case MenuType.Field:
                        inputReceiver.SwitchField();
                        break;
                    case MenuType.Menu:
                        inputReceiver.SwitchMenu();
                        break;
                }
            });
        }

        private MenuType? GetCurrentMenuType()
        {
            if (_menuStack.Count == 0)
                return null;
            return _menuStack.Peek() == _dungeonMenu ? MenuType.Field : MenuType.Menu;
        }

        public async UniTask<int> GetChoice(string? text, params string[] choices)
        {
            _choiceMenu.SetChoices(text, choices);
            await UniTask.NextFrame();
            AddMenu(_choiceMenu);
            var selectedIndex = await _choiceMenu.SelectedIndex.WaitAsync();
            PopMenu();
            return selectedIndex;
        }

        public async UniTask<string> GetTextInput()
        {
            AddMenu(_textInputMenu);
            var text = await _textInputMenu.Text.WaitAsync();
            PopMenu();
            return text;
        }

        public void SwitchMenu(IMenu menu)
        {
            Log.Info($"[Menu]SwitchMenu: {menu}");
            if (_menuStack.Count > 0)
            {
                var previousMenu = _menuStack.Peek();
                _selectedObject[previousMenu] = EventSystem.current.currentSelectedGameObject;
                previousMenu.Hide();
                previousMenu.Disable();
            }

            if (_selectedObject.ContainsKey(menu))
                EventSystem.current.SetSelectedGameObject(_selectedObject[menu]);
            menu.Show();
            menu.Enable();
            _menuStack.Clear();
            _menuStack.Push(menu);
        }

        public void PushMenu(IMenu pushedMenu)
        {
            Log.Info($"[Menu]PushMenu: {pushedMenu}");
            if (_menuStack.Count > 0)
            {
                var previousMenu = _menuStack.Peek();
                _selectedObject[previousMenu] = EventSystem.current.currentSelectedGameObject;
                previousMenu.Hide();
                previousMenu.Disable();
            }

            EventSystem.current.SetSelectedGameObject(_selectedObject.GetValueOrDefault(pushedMenu));
            pushedMenu.Show();
            pushedMenu.Enable();
            _menuStack.Push(pushedMenu);
        }

        public void AddMenu(IMenu addedMenu)
        {
            Log.Info($"[Menu]AddMenu: {addedMenu}");
            if (_menuStack.Count > 0)
            {
                var previousMenu = _menuStack.Peek();
                _selectedObject[previousMenu] = EventSystem.current.currentSelectedGameObject;
                previousMenu.Disable();
            }

            EventSystem.current.SetSelectedGameObject(_selectedObject.GetValueOrDefault(addedMenu));
            addedMenu.Show();
            addedMenu.Enable();
            _menuStack.Push(addedMenu);
        }

        public void PopMenu()
        {
            var poppedMenu = _menuStack.Pop();
            Log.Info($"[Menu]PopMenu: {poppedMenu}");
            _selectedObject[poppedMenu] = EventSystem.current.currentSelectedGameObject;
            if (_menuStack.Count > 0)
            {
                var previousMenu = _menuStack.Peek();
                EventSystem.current.SetSelectedGameObject(_selectedObject.GetValueOrDefault(previousMenu));
                previousMenu.Show();
                previousMenu.Enable();
            }

            poppedMenu.Hide();
            poppedMenu.Disable();
        }

        public void TitleMenu()
        {
            SwitchMenu(_titleMenu);
        }

        public void TitleMenuWhenGameOver(int level, string causeOfDeath)
        {
            SwitchMenu(_titleMenu);
            _titleMenu.SetData(level, causeOfDeath);
        }

        public void DungeonMenu()
        {
            SwitchMenu(_dungeonMenu);
        }

        public void PushSettingMenu()
        {
            PushMenu(_settingMenu);
        }

        public void PushItemLibraryMenu()
        {
            PushMenu(_itemLibraryMenu);
        }
    }
}