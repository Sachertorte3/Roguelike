#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Message;
using ObservableCollections;
using R3;
using Utilities;

namespace Domain.Service.Items
{
    internal class Inventory : ISerializable<InventoryMemento>, IInventory, IDisposable
    {
        private const int MaxItems = 10;
        private readonly IDisposable _disposable;
        private readonly CompositeDisposable[] _disposables = EnumerableExtension.CreateArrayWithNewInstances<CompositeDisposable>(MaxItems).ToArray();
        private readonly ObservableList<IItem?> _items = new(Enumerable.Repeat<Item?>(null, MaxItems));
        public IEnumerable<IItem> AllItems => _items.Where(item => item != null).Cast<IItem>();
        private readonly Subject<OnItemUpdated> _onItemUpdated = new();

        public Inventory(InventoryMemento data)
        {
            _disposable = OnItemChanged.Subscribe(itemChanged =>
                {
                    _disposables[itemChanged.Index].Clear();
                    if (itemChanged.NewValue != null)
                    {
                        _disposables[itemChanged.Index].Add(itemChanged.NewValue.OnItemUpdated.Subscribe(
                            _ => _onItemUpdated.OnNext(new OnItemUpdated(itemChanged.NewValue, itemChanged.Index))
                        ));
                        _disposables[itemChanged.Index].Add(itemChanged.NewValue.RemainingUses.Subscribe(
                            remainingUses =>
                            {
                                if (remainingUses <= 0)
                                    Replace(null, itemChanged.Index);
                            }
                        ));
                    }
                }
            );

            foreach (var item in data.Items)
            {
                if (item != null)
                    _items[_items.IndexOf(null)] = new Item(item);
            }
        }

        public void Dispose()
        {
            _disposable.Dispose();
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }

        public int MaxItemCount => MaxItems;

        public Observable<CollectionReplaceEvent<IItem?>> OnItemChanged => _items.ObserveReplace();
        public Observable<OnItemUpdated> OnItemUpdated => _onItemUpdated;

        public bool HasEmptySpace()
        {
            return _items.IndexOf(null) >= 0;
        }

        public IItem? GetItem(int index)
        {
            return _items[index];
        }

        public InventoryMemento Serialize()
        {
            return new InventoryMemento(_items.Select(x => x?.Serialize()).ToArray());
        }

        public bool TryAdd(IItem item)
        {
            var index = _items.IndexOf(null);
            if (index >= 0)
            {
                Replace(item, index);
                return true;
            }

            return false;
        }

        public IItem? Replace(IItem? item, int index)
        {
            var removed = _items[index];
            _items[index] = item;
            return removed;
        }

        public IItem? Remove(int index)
        {
            return Replace(null, index);
        }

        public void RepairAll()
        {
            foreach (var item in _items)
            {
                item?.Repair();
            }
        }
    }
}