#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Items;
using Domain.Model.Map;
using Domain.Model.Message;
using R3;
using Utilities;

namespace Domain.Model
{
    public interface IItemEntity : IDisposable, ISerializable<ItemEntityMemento>, IIconEntity
    {
        public IItem Item { get; }
        public Observable<OnEffectSpawnedMessage> OnEffectSpawned { get; }
        public Observable<Unit> OnDisabled { get; }

        public UniTask Throw(IActor actor, Direction8 direction, IMap map);
    }
}