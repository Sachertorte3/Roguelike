#nullable enable
using ObservableCollections;
using R3;

namespace Model.Items
{
    public interface IInventory
    {
        public Observable<CollectionReplaceEvent<Item?>> OnItemChanged { get; }
        public Observable<OnItemUpdated> OnItemUpdated { get; }
        public bool HasEmptySpace();
        public Item? GetItem(int index);
    }
}