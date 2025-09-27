#nullable enable
using System.Collections.Generic;
using Domain.Model.Character.Message;
using R3;
using Utilities.Serialize.Result;

namespace Domain.Model.Item
{
    public interface IStorage
    {
        public IEnumerable<IItem> AllItems { get; }
        public IEnumerable<(IItem Item, int Index)> AllItemsWithIndex { get; }
        public IEnumerable<IItem> AllItemsRecursive { get; }
        public int Capacity { get; }
        public bool CanRemoveItem { get; }
        public Observable<OnItemChanged> OnItemChanged { get; }
        public Observable<OnItemUpdated> OnItemUpdated { get; }
        public Observable<OnItemOverflowed> OnItemOverflowed { get; }
        public bool HasEmptySpace();
        public IItem? GetItem(int index);
        public bool HasItemAt(int index, out IItem item);
        public int GetItemIndex(IItem? item);
        public void Add(IItem item);
        public bool TryAdd(IItem item);
        public void Remove(IItem item);
        public bool TryRemove(IItem item);
        public Result<IItem?> Replace(IItem? item, int index);
        public IEnumerable<IItem> Clear();
    }
}