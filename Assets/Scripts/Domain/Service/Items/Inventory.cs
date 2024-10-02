#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Item;
using Domain.Model.Memento;
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
        private readonly CompositeDisposable[] _disposables = EnumerableExtension.CreateNewInstances<CompositeDisposable>(MaxItems).ToArray();
        private readonly ObservableList<IItem?> _items = new(Enumerable.Repeat<Item?>(null, MaxItems));
        public IEnumerable<IItem> AllItems => _items.Where(item => item != null).Cast<IItem>();
        private readonly Subject<OnItemUpdated> _onItemUpdated = new();
        private IHasCondition _hasCondition;

        public Inventory(InventoryMemento data, IHasCondition hasCondition)
        {
            _hasCondition = hasCondition;

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

            for (var i = 0; i < MaxItems; i++)
            {
                _items[i] = data.Items[i].Map(item => new Item(item)).Value;
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

        public void UpdateTurn()
        {
            foreach (var item in _items)
            {
                if (item == null)
                    continue;
                foreach (var condition in item.PassiveConditions)
                {
                    condition.Persist(_hasCondition);
                }
            }
        }

        public bool HasEmptySpace()
        {
            return _items.IndexOf(null) >= 0;
        }

        public IItem? GetItem(int index)
        {
            return _items[index];
        }

        public int GetItemIndex(IItem item)
        {
            return _items.IndexOf(item);
        }

        public InventoryMemento Serialize()
        {
            return new InventoryMemento
            (
                items: _items.Select(x => x.ToOption().Map(x => x.Serialize())).ToArray()
            );
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
            if (item != null)
            {
                foreach (var condition in item.PassiveConditions)
                    condition.Inflict(_hasCondition, Id<IEntity>.Empty);
            }
            if (removed != null)
            {
                foreach (var condition in removed.PassiveConditions)
                    condition.Delete(_hasCondition, Id<IEntity>.Empty);
            }
            return removed;
        }

        public IItem? Remove(int index)
        {
            return Replace(null, index);
        }

        public IItem? Remove(IItem item)
        {
            var index = _items.IndexOf(item);
            if (index < 0)
                return null;
            return Replace(null, index);
        }
    }
}