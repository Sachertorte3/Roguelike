#nullable enable
using System.Collections;
using System.Collections.Generic;
using Domain.Model.Message;
using ObservableCollections;
using R3;

namespace Domain.Model.Items
{
    public interface IInventory
    {
        public IEnumerable<IItem> AllItems { get; }
        public int MaxItemCount { get; }
        public Observable<CollectionReplaceEvent<IItem?>> OnItemChanged { get; }
        public Observable<OnItemUpdated> OnItemUpdated { get; }
        public bool HasEmptySpace();
        public IItem? GetItem(int index);
    }
}