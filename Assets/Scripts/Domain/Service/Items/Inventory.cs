#nullable enable
using System;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Items;
using Domain.Model.Message;
using ObservableCollections;
using R3;

namespace Domain.Service.Items
{
    internal class Inventory : ISerializable<InventoryMemento>, IInventory, IDisposable
    {
        private const int MaxItems = 10;
        private readonly IDisposable _disposable;
        private readonly CompositeDisposable _disposables = new();
        private readonly ObservableList<IItem?> _items = new(Enumerable.Repeat<Item?>(null, MaxItems));
        private readonly Subject<OnItemUpdated> _onItemUpdated = new();

        public Inventory(InventoryMemento data)
        {
            _disposable = OnItemChanged.Subscribe(itemChanged =>
                {
                    if (itemChanged.NewValue != null)
                    {
                        _disposables.Add(itemChanged.NewValue.RemainingUses.Subscribe(
                            remainingUses =>
                            {
                                if (remainingUses <= 0)
                                    Replace(null, _items.IndexOf(itemChanged.NewValue));
                                else if (itemChanged.NewValue != null)
                                    _onItemUpdated.OnNext(new OnItemUpdated(itemChanged.NewValue, itemChanged.Index));
                            }));
                    }
                },
                _ => _disposables.Clear());
            foreach (var item in data.Items)
            {
                if (item != null)
                    _items[_items.IndexOf(null)] = new Item(item);
            }
        }

        public void Dispose()
        {
            _disposable.Dispose();
            _disposables.Dispose();
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