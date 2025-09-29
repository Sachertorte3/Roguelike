#nullable enable
using System.Collections.Generic;
using Domain.Model.Character.Message;
using R3;

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
        public bool HasItemAt(int index);
        public bool HasItemAt(int index, out IItem item);
        public bool Contains(IItem item);
        public IItem? GetItem(int index);
        public int GetItemIndex(IItem? item);
        public IStorage? GetItemStorage(int index);
        public bool CanAddToEmpty(IItem item);
        public void AddToEmpty(IItem item);
        public bool CanAdd(IItem item, int index);
        public void Add(IItem item, int index);
        public bool CanAddOrNot(IItem? item, int index);
        public void AddOrNot(IItem? item, int index);
        public bool CanRemove(int index);
        public IItem? Remove(int index);
        public bool CanRemove(IItem item);
        public void Remove(IItem item);
        public bool CanReplace(IItem item, int index);
        public IItem? Replace(IItem item, int index);
        public bool CanReplaceOrRemove(IItem? item, int index);
        public IItem? ReplaceOrRemove(IItem? item, int index);
        public IEnumerable<IItem> Clear();
    }
}