#nullable enable
using System;
using Domain.Model.Entity;
using Domain.Model.Memento;
using R3;

namespace Domain.Model.Item
{
    public interface IItemEntity : IDisposable, ISerializable<ItemEntityMemento>, IIconEntity
    {
        public IItem Item { get; }
        public Observable<Unit> OnDisabled { get; }
    }
}