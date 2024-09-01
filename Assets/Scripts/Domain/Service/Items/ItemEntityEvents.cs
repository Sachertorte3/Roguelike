#nullable enable
using System;
using Domain.Model;
using Domain.Model.Message;
using Domain.Service.Entities;
using R3;
using Utilities.Messages;

namespace Domain.Service.Items
{
    public class ItemEntityEvents : IDisposable, IEntityGroupEvents
    {
        private readonly GroupEvents<IItemEntity> _events = new();

        public Observable<(IItemEntity Item, OnPositionChangedMessage Message)> OnPositionChanged =>
            _events.GetObservable<OnPositionChangedMessage>();

        public Observable<(IItemEntity Item, OnDisabledMessage Message)> OnDisabled =>
            _events.GetObservable<OnDisabledMessage>();

        public Observable<(IItemEntity Item, OnMoveMessage Message)> OnMove => _events.GetObservable<OnMoveMessage>();

        public Observable<(IItemEntity Item, OnTeleportMessage Message)> OnTeleport =>
            _events.GetObservable<OnTeleportMessage>();

        public Observable<(IItemEntity Item, OnDestroyedMessage Message)> OnDestroyed =>
            _events.GetObservable<OnDestroyedMessage>();

        public Observable<(IItemEntity Item, OnEffectSpawnedMessage Message)> OnEffectSpawned =>
            _events.GetObservable<OnEffectSpawnedMessage>();

        public void Dispose()
        {
            _events.Dispose();
        }

        Observable<(IEntity Entity, OnPositionChangedMessage Message)> IEntityGroupEvents.OnPositionChanged =>
            _events.GetSubject<OnPositionChangedMessage>().SelectSender(item => (IEntity)item);

        Observable<(IEntity Entity, OnMoveMessage Message)> IEntityGroupEvents.OnMove =>
            _events.GetSubject<OnMoveMessage>().SelectSender(item => (IEntity)item);

        Observable<(IEntity Entity, OnTeleportMessage Message)> IEntityGroupEvents.OnTeleport =>
            _events.GetSubject<OnTeleportMessage>().SelectSender(item => (IEntity)item);

        Observable<(IEntity Entity, OnDestroyedMessage Message)> IEntityGroupEvents.OnDestroyed =>
            _events.GetSubject<OnDestroyedMessage>().SelectSender(item => (IEntity)item);

        ~ItemEntityEvents()
        {
            Dispose();
        }

        public void Add(IItemEntity item)
        {
            _events.Add(item, item.Position.Select(positionChanged => new OnPositionChangedMessage(positionChanged)));
            _events.Add(item, item.OnDisabled.Select(disabled => new OnDisabledMessage()));
            _events.Add(item, item.OnMove.Select(move => new OnMoveMessage(move.direction, move.destination)));
            _events.Add(item, item.OnTeleport.Select(teleport => new OnTeleportMessage(teleport)));
            _events.Add(item,
                item.OnEffectSpawned.Select(useSkill => new OnEffectSpawnedMessage(useSkill.Area, useSkill.Color)));
            _events.Add(item, item.OnDestroyed.Select(destroyed => new OnDestroyedMessage()));
        }

        public void Remove(IItemEntity character)
        {
            _events.Remove(character);
        }
    }
}