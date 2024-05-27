#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;

namespace Model.Domain.Items
{
    internal class Inventory : IInventory, IDisposable
    {
        private const int MaxItems = 10;
        private readonly ObservableList<Item?> _items = new(Enumerable.Repeat<Item?>(null, MaxItems));
        private readonly Subject<OnItemUpdated> _onItemUpdated = new();

        private readonly List<SerialDisposable> disposables =
            new(Enumerable.Range(0, MaxItems).Select(_ => new SerialDisposable()));

        public Inventory()
        {
            OnItemChanged.Subscribe(itemChanged =>
            {
                disposables[itemChanged.Index].Disposable = itemChanged.NewValue?.RemainingUses.Subscribe(
                    remainingUses =>
                    {
                        if (remainingUses <= 0)
                            Replace(null, _items.IndexOf(itemChanged.NewValue));
                        else if (itemChanged.NewValue != null)
                            _onItemUpdated.OnNext(new OnItemUpdated(itemChanged.NewValue, itemChanged.Index));
                    });
            });
        }

        public void Dispose()
        {
            foreach (var item in disposables) item?.Dispose();
        }

        public Observable<CollectionReplaceEvent<Item?>> OnItemChanged => _items.ObserveReplace();

        public Observable<OnItemUpdated> OnItemUpdated => _onItemUpdated;

        public bool HasEmptySpace()
        {
            return _items.IndexOf(null) >= 0;
        }

        public Item? GetItem(int index)
        {
            return _items[index];
        }

        public bool TryAdd(Item item)
        {
            var index = _items.IndexOf(null);
            if (index >= 0)
            {
                Replace(item, index);
                return true;
            }

            return false;
        }

        public Item? Replace(Item? item, int index)
        {
            var removed = _items[index];
            _items[index] = item;
            return removed;
        }

        public Item? Remove(int index)
        {
            return Replace(null, index);
        }
    }
}