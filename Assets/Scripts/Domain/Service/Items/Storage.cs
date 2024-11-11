#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Message;
using Domain.Model.Item;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using Utilities;
using Utilities.Serialize;

namespace Domain.Service.Items
{
    internal class Storage : IStorage, IDisposable, ISerializable<StorageMemento>
    {
        public int Capacity => _items.Count;
        private readonly ObservableList<IItem?> _items;
        public IEnumerable<IItem> AllItems => _items.Where(item => item != null).Cast<IItem>();
        private readonly Subject<OnItemUpdated> _onItemUpdated = new();

        public Observable<CollectionReplaceEvent<IItem?>> OnItemChanged => _items.ObserveReplace();
        public Observable<OnItemUpdated> OnItemUpdated => _onItemUpdated;

        private readonly IDisposable _disposable;
        private readonly CompositeDisposable[] _disposables;

        public Storage(StorageMemento data)
        {
            _items = new ObservableList<IItem?>();
            foreach (var item in data.Items)
            {
                _items.Add(item.Map(item => new Item(item)).Value);
            }

            _disposables = EnumerableExtension.CreateNewInstances<CompositeDisposable>(Capacity).ToArray();

            _disposable = _items.ObserveReplace().Subscribe(itemChanged =>
            {
                _disposables[itemChanged.Index].Clear();
                if (itemChanged.NewValue != null)
                {
                    _disposables[itemChanged.Index].Add(
                        itemChanged.NewValue.OnItemUpdated.Subscribe(
                            _ => _onItemUpdated.OnNext(new OnItemUpdated(itemChanged.NewValue, itemChanged.Index))
                        ));
                }
            });
        }

        public void Dispose()
        {
            _onItemUpdated.Dispose();
            _disposable.Dispose();
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }

        public StorageMemento Serialize()
        {
            return new StorageMemento
            (
                _items.Select(x => x.ToOption().Map(x => x.Serialize())).ToArray()
            );
        }

        public static StorageMemento Build(IItem?[] items)
        {
            return new StorageMemento(items.Select(item => item.ToOption().Map(item => item.Serialize())).ToArray());
        }
        public static StorageMemento Build(int capacity)
        {
            var itemArray = EnumerableExtension.CreateNewInstances<Option<ItemMemento>>(capacity).ToArray();
            return new StorageMemento(itemArray);
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

        public bool TryRemove(IItem item)
        {
            var index = _items.IndexOf(item);
            if (index >= 0)
            {
                Replace(null, index);
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

        public bool Remove(IItem item)
        {
            var index = _items.IndexOf(item);
            if (index < 0)
                return false;
            return Replace(null, index) != null;
        }
    }
}