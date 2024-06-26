#nullable enable
using ObservableCollections;
using R3;

namespace Domain.Service.Items
{
    public interface IInventory
    {
        public int MaxItemCount { get; }
        public Observable<CollectionReplaceEvent<Item?>> OnItemChanged { get; }
        public Observable<OnItemUpdated> OnItemUpdated { get; }
        public bool HasEmptySpace();
        public Item? GetItem(int index);
    }
}