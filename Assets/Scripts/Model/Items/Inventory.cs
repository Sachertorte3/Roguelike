#nullable enable
using ObservableCollections;
using R3;
using Scripts.Model.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Assets.Scripts.Model.Items
{
    internal class Inventory : IInventory, IDisposable
    {
        private const int MaxItems = 10;
        public ReadOnlyCollection<Item?> Items => new(_items);
        public Observable<CollectionReplaceEvent<Item?>> OnItemChanged => _items.ObserveReplace();
        private ObservableList<Item?> _items = new(Enumerable.Repeat<Item?>(null, MaxItems));
        public bool HasEmptySpace() => _items.IndexOf(null) >= 0;
        public Observable<OnItemUpdated> OnItemUpdated => _onItemUpdated;
        private Subject<OnItemUpdated> _onItemUpdated = new();
        private List<IDisposable?> disposables = new(Enumerable.Repeat<IDisposable?>(null, MaxItems));
        public Inventory()
        {
            OnItemChanged.Subscribe(itemChanged =>
            {
                disposables[itemChanged.Index]?.Dispose();
                disposables[itemChanged.Index] = itemChanged.NewValue?.RemainingUses.Subscribe(remainingUses =>
                {
                    if (remainingUses <= 0)
                    {
                        Replace(null, _items.IndexOf(itemChanged.NewValue));
                    }
                    else if (itemChanged.NewValue != null)
                    {
                        _onItemUpdated.OnNext(new OnItemUpdated(itemChanged.NewValue, itemChanged.Index));
                    }
                });
            });
        }
        public void Dispose()
        {
            foreach (var item in disposables)
            {
                item?.Dispose();
            }
        }
        public Item? GetItem(int index)
        {
            return Items[index];
        }
        public bool TryAdd(Item item)
        {
            int index = _items.IndexOf(null);
            if (index >= 0)
            {
                Replace(item, index);
                return true;
            }
            else
            {
                return false;
            }
        }
        public Item? Replace(Item? item, int index)
        {
            Item? removed = _items[index];
            _items[index] = item;
            return removed;
        }
        public Item? Remove(int index)
        {
            return Replace(null, index);
        }
    }
    public class InventoryIndexReceiver
    {
        public int Index { get; private set; } = -1;
        public void SetIndex(int index)
        {
            Index = index;
        }
    }
    public record OnItemUpdated(Item Item, int Index);
}
