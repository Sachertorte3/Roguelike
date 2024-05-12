#nullable enable
using ObservableCollections;
using R3;
using Scripts.Model.Items;
using System.Collections.ObjectModel;

namespace Assets.Scripts.Model.Items
{
    public interface IInventory
    {
        public bool HasEmptySpace();
        public ReadOnlyCollection<Item?> Items { get; }
        public Observable<CollectionReplaceEvent<Item?>> OnItemChanged { get; }
        public Observable<OnItemUpdated> OnItemUpdated { get; }
    }
}
