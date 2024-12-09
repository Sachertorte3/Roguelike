#nullable enable
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
                var itemData = ScriptableObjectLoader.Load<ItemData>(collectionChanged.NewItem);
                var itemViewData = new ItemLibraryViewData(collectionChanged.NewItem, itemData.Icon, itemData.IsShiny, new Item(itemData).FullInfo());
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