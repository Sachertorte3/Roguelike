#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Items;
using Domain.Model.Map;
using Domain.Model.Message;
using Domain.Service.Entities;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service
{
    public interface IItemEntity : IDisposable, ISerializable<ItemEntityMemento>, IEntity
    {
        public IItem Item { get; }
        public Sprite Icon => Item.Icon;
        public Observable<OnEffectSpawnedMessage> OnEffectSpawned { get; }
        public Observable<Unit> OnDisabled { get; }

        public UniTask Throw(IActor actor, Direction8 direction, IMap map);
    }
}