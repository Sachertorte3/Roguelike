using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace View.UI
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private GameObject _ui;
        [SerializeField] private GameObject _menu;
        private readonly Dictionary<GameObject, GameObject> _selectedObject = new();

        [Inject]
        public void Construct(InputReceiver inputReceiver)
        {
            inputReceiver.OnMenuOpening.Subscribe(_ =>
            {
                _selectedObject[_ui] = EventSystem.current.currentSelectedGameObject;
                EventSystem.current.SetSelectedGameObject(_selectedObject.GetValueOrDefault(_menu));
                _ui.SetActive(false);
                _menu.SetActive(true);
            });
            inputReceiver.OnMenuClosing.Subscribe(_ =>
            {
                _selectedObject[_menu] = EventSystem.current.currentSelectedGameObject;
                EventSystem.current.SetSelectedGameObject(_selectedObject.GetValueOrDefault(_ui));
                _ui.SetActive(true);
                _menu.SetActive(false);
            });
        }
    }
}