#nullable enable
using System.Collections.Generic;
using Domain.Model.Character.Message;
using Domain.Model.Item;
using ObservableCollections;
using R3;

namespace Domain.Model.Character
{
    public interface IInventory
    {
        public IEnumerable<IItem> AllItems { get; }
        public int MaxItemCount { get; }
        public Observable<CollectionReplaceEvent<IItem?>> OnItemChanged { get; }
        public Observable<OnItemUpdated> OnItemUpdated { get; }
        public bool HasEmptySpace();
        public IItem? GetItem(int index);
        public int GetItemIndex(IItem item);
        public bool TryAdd(IItem item);
        public IItem? Replace(IItem? item, int index);
    }
}