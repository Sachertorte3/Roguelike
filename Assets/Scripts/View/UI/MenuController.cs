#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private SettingMenu _settingMenu;
        [SerializeField] private ChoiceMenu _choiceMenu;
        private readonly Stack<IMenu> _menuStack = new();
        private readonly Dictionary<IMenu, GameObject> _selectedObject = new();

        [Inject]
        public void Construct(InputReceiver inputReceiver)
        {
            inputReceiver.OnMenuOpening.Subscribe(_ =>
            {
                AddMenu(_settingMenu);
            });
            inputReceiver.OnMenuClosing.Subscribe(_ =>
            {
                PopMenu();
            });
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

        public void SwitchMenu(IMenu menu)
        {
            Log.Info($"SwitchMenu: {menu}");
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
            Log.Info($"PushMenu: {pushedMenu}");
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
            Log.Info($"AddMenu: {addedMenu}");
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
            Log.Info($"PopMenu: {poppedMenu}");
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

        public void DungeonMenu()
        {
            SwitchMenu(_dungeonMenu);
        }
    }
}