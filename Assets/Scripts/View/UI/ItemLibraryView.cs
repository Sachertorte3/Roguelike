#nullable enable
using System.Collections.Generic;
using System.Linq;
using R3;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Logging;
using Utilities;

namespace View.UI
{
    public record ItemLibraryViewData(string Name, Sprite Icon, int Category, bool IsShiny, string Info);
    public class ItemLibraryView : MonoBehaviour, IMenu
    {
        public bool CanClose => true;
        [SerializeField] private GameObject _content;
        [SerializeField] private InventoryItemView _itemViewPrefab;
        [SerializeField] private TMP_Text _infoText;
        private readonly List<ItemLibraryViewData> _items = new();
        private readonly List<InventoryItemView> _itemViews = new();

        public void AddItem(string name, ItemLibraryViewData itemViewData)
        {
            var index = _items.FindIndex(item => item.Name == name);
            if (index != -1)
                _items[index] = itemViewData;
            else
                _items.Add(itemViewData);
            GenerateViews();
        }

        private void GenerateViews()
        {
            Log.Debug($"GenerateViews: {_items.Count}");
            foreach (var view in _itemViews.WhereNotNull())
            {
                Destroy(view.gameObject);
            }
            _itemViews.Clear();
            foreach (var itemData in _items.OrderBy(item => item.Category).ThenBy(item => item.Name))
            {
                var view = Instantiate(_itemViewPrefab, _content.transform);
                view.Set(itemData.Icon, null, false, itemData.IsShiny, true, true);
                _itemViews.Add(view);
            }

            _itemViews.ForEach((view, index) => view.OnSelected.Subscribe(_ =>
            {
                _infoText.text = _items[index].Info;
            }).AddTo(view));

            for (var i = 0; i < _itemViews.Count; i++)
                SetNavigation(i);
        }

        public void SetNavigation(int index)
        {
            var nav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = _itemViews[(index - 1 + _itemViews.Count) % _itemViews.Count]
                    .GetComponent<Selectable>(),
                selectOnRight = _itemViews[(index + 1) % _itemViews.Count].GetComponent<Selectable>()
            };
            _itemViews[index].GetComponent<Selectable>().navigation = nav;
        }

        public void Show()
        {
            gameObject.SetActive(true);
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