#nullable enable
using System;
using Domain.Model;
using Domain.Service.Entities;
using Domain.Service.Events;
using R3;
using Utilities.Messages;

namespace Domain.Service.Items
{
    public class FireEntityEvents : IDisposable, IEntityGroupEvents
    {
        private readonly GroupEvents<Fire> _events = new();

        public Observable<(Fire Entity, OnPositionChangedMessage Message)> OnPositionChanged =>
            _events.GetObservable<OnPositionChangedMessage>();

        public Observable<(Fire Entity, OnMoveMessage Message)> OnMove =>
            _events.GetObservable<OnMoveMessage>();

        public Observable<(Fire Entity, OnTeleportMessage Message)> OnTeleport =>
            _events.GetObservable<OnTeleportMessage>();

        public Observable<(Fire Entity, OnDestroyedMessage Message)> OnDestroyed =>
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

        ~FireEntityEvents()
        {
            Dispose();
        }

        public void Add(Fire entity)
        {
            _events.Add(entity,
                entity.Position.Select(positionChanged => new OnPositionChangedMessage(positionChanged)));
            _events.Add(entity, entity.OnMove.Select(move => new OnMoveMessage(move.direction, move.destination)));
            _events.Add(entity, entity.OnTeleport.Select(teleport => new OnTeleportMessage(teleport)));
            _events.Add(entity, entity.OnDestroyed.Select(destroyed => new OnDestroyedMessage()));
        }

        public void Remove(Fire entity)
        {
            _events.Remove(entity);
        }
    }
}