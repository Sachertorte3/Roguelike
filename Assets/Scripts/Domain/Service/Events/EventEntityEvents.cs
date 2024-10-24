#nullable enable
using System;
using Domain.Model;
using Domain.Service.Entities;
using R3;
using Utilities.Messages;

namespace Domain.Service.Items
{
    public class EventEntityEvents : IDisposable, IEntityGroupEvents
    {
        private readonly GroupEvents<IEventEntity> _events = new();

        public Observable<(IEventEntity EventEntity, OnPositionChangedMessage Message)> OnPositionChanged =>
            _events.GetObservable<OnPositionChangedMessage>();

        public Observable<(IEventEntity EventEntity, OnMoveMessage Message)> OnMove =>
            _events.GetObservable<OnMoveMessage>();

        public Observable<(IEventEntity EventEntity, OnTeleportMessage Message)> OnTeleport =>
            _events.GetObservable<OnTeleportMessage>();

        public Observable<(IEventEntity EventEntity, OnDestroyedMessage Message)> OnDestroyed =>
            _events.GetObservable<OnDestroyedMessage>();

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

        ~EventEntityEvents()
        {
            Dispose();
        }

        public void Add(IEventEntity eventEntity)
        {
            _events.Add(eventEntity,
                eventEntity.Position.Select(positionChanged => new OnPositionChangedMessage(positionChanged)));
            _events.Add(eventEntity,
                eventEntity.OnMove.Select(move => new OnMoveMessage(move.direction, move.destination)));
            _events.Add(eventEntity, eventEntity.OnTeleport.Select(teleport => new OnTeleportMessage(teleport)));
            _events.Add(eventEntity, eventEntity.OnDestroyed.Select(destroyed => new OnDestroyedMessage()));
        }

        public void Remove(IEventEntity character)
        {
            _events.Remove(character);
        }
    }
}