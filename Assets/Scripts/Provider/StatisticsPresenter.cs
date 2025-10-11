#nullable enable
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Service.Items;
using Game;
using ObservableCollections;
using R3;
using Utilities;
using VContainer;
using View.UI;

namespace Provider
{
    public class StatisticsPresenter
    {
        [Inject]
        public StatisticsPresenter(GameManager gameManager, ItemLibraryView itemLibraryView)
        {
            var _disposable = new SerialDisposable();
            var knownItemNames = new ObservableHashSet<string>();

            knownItemNames.ObserveChanged().Subscribe(collectionChanged =>
            {
                var baseItemData = ScriptableObjectLoaderExtension.LoadItemData(collectionChanged.NewItem);
                var itemViewData = baseItemData.Match(
                    itemData => new ItemLibraryViewData(collectionChanged.NewItem, itemData.Icon, (int)itemData.Category, itemData.IsShiny, new Item(itemData).FullInfo()),
                    directWeaponData => new ItemLibraryViewData(collectionChanged.NewItem, directWeaponData.Icon, (int)ItemCategory.Weapons, directWeaponData.IsShiny, new DirectWeapon(directWeaponData).FullInfo())
                );
                itemLibraryView.AddItem(collectionChanged.NewItem, itemViewData);
            });

            gameManager.ActiveStatistics.SubscribeIncludingCurrentValueIgnoreNull(statistics =>
            {
                foreach (var itemName in statistics.KnownItemNames)
                {
                    knownItemNames.Add(itemName);
                }
                statistics.KnownItemNames.ObserveChanged().Subscribe(itemName =>
                {
                    if (itemName.NewItem != null)
                    {
                        knownItemNames.Add(itemName.NewItem);
                    }
                });
            });
        }
    }
}