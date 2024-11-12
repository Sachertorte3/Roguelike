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
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;
using Utilities.Serialize.Result;
using Result = Utilities.Serialize.Result.Result;

namespace Domain.Service.Items
{
    internal class Storage : IStorage, IDisposable, ISerializable<StorageMemento>
    {
        public int Capacity => _items.Count;
        private readonly ObservableList<IItem?> _items;
        public IEnumerable<IItem> AllItems => _items.Where(item => item != null).Cast<IItem>();
        private readonly bool _canAddItemsWithStorage;
        private readonly Subject<OnItemUpdated> _onItemUpdated = new();

        public Observable<CollectionReplaceEvent<IItem?>> OnItemChanged => _items.ObserveReplace();
        public Observable<OnItemUpdated> OnItemUpdated => _onItemUpdated;

        private readonly IDisposable _disposable;
        private readonly CompositeDisposable[] _disposables;

        public Storage(StorageMemento data)
        {
            _items = new ObservableList<IItem?>();
            for (var i = 0; i < data.Items.Length; i++)
            {
                //MEMO: Since you are only subscribed to Replace, additions must also be done using Replace.
                _items.Add(null);
            }
            _canAddItemsWithStorage = data.CanAddItemsWithStorage;

            _disposables = EnumerableExtension.CreateNewInstances<CompositeDisposable>(Capacity).ToArray();
            _disposable = _items.ObserveReplace().Subscribe(itemChanged =>
            {
                _disposables[itemChanged.Index].Clear();
                if (itemChanged.NewValue != null)
                {
                    itemChanged.NewValue.OnItemUpdated.Subscribe(
                        _ => _onItemUpdated.OnNext(new OnItemUpdated(itemChanged.NewValue, itemChanged.Index))
                    ).AddTo(_disposables[itemChanged.Index]);

                    itemChanged.NewValue.RemainingUses.Subscribe(
                        remainingUses =>
                        {
                            if (remainingUses <= 0 && itemChanged.NewValue.AutoDestroyWhenDisabled)
                                Remove(itemChanged.Index);
                        }
                    ).AddTo(_disposables[itemChanged.Index]);
                }
            });

            for (var i = 0; i < data.Items.Length; i++)
            {
                Replace(data.Items[i].Map(item => new Item(item)).Value, i);
            }
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
                _items.Select(x => x.ToOption().Map(x => x.Serialize())).ToArray(),
                _canAddItemsWithStorage
            );
        }

        public static StorageMemento Build(IItem?[] items, bool canAddItemsWithStorage)
        {
            return new StorageMemento(
                items.Select(item => item.ToOption().Map(item => item.Serialize())).ToArray(),
                canAddItemsWithStorage
            );
        }

        public static StorageMemento Build(int capacity, bool canAddItemsWithStorage)
        {
            var itemArray = EnumerableExtension.CreateNewInstances<Option<ItemMemento>>(capacity).ToArray();
            return new StorageMemento(itemArray, canAddItemsWithStorage);
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
            if (!_canAddItemsWithStorage && item.ItemStorage.IsSome)
                return false;
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
                Remove(index);
                return true;
            }
            return false;
        }

        public Result<IItem?> Replace(IItem? item, int index)
        {
            if (!_canAddItemsWithStorage && item != null && item.ItemStorage.IsSome)
                return Result<IItem?>.Error;
            var removed = _items[index];
            _items[index] = item;

            return Result.Ok(removed);
        }

        public IItem? Remove(int index)
        {
            return Replace(null, index).Value;
        }

        public bool Remove(IItem item)
        {
            var index = _items.IndexOf(item);
            if (index < 0)
                return false;
            return Remove(index) != null;
        }
    }
}