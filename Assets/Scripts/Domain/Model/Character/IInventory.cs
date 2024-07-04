#nullable enable
using Domain.Model.Message;
using ObservableCollections;
using R3;

namespace Domain.Model.Items
{
    public interface IInventory
    {
        public int MaxItemCount { get; }
        public Observable<CollectionReplaceEvent<IItem?>> OnItemChanged { get; }
        public Observable<OnItemUpdated> OnItemUpdated { get; }
        public bool HasEmptySpace();
        public IItem? GetItem(int index);
    }
}