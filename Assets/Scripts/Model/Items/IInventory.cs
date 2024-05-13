#nullable enable
using System.Collections.ObjectModel;
using ObservableCollections;
using R3;
using Scripts.Model.Items;

namespace Assets.Scripts.Model.Items
{
    public interface IInventory
    {
        public bool HasEmptySpace();
        public ReadOnlyCollection<Item?> Items { get; }
        public Observable<CollectionReplaceEvent<Item?>> OnItemChanged { get; }
        public Observable<OnItemUpdated> OnItemUpdated { get; }
        public Item? GetItem(int index);
    }
}
