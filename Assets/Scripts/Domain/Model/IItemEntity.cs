#nullable enable
using System;
using Domain.Model.Item;
using Domain.Model.Memento;
using R3;

namespace Domain.Model
{
    public interface IItemEntity : IDisposable, ISerializable<ItemEntityMemento>, IIconEntity
    {
        public IItem Item { get; }
        public Observable<Unit> OnDisabled { get; }
    }
}