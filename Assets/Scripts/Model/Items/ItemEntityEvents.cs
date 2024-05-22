#nullable enable
using Model.Characters;
using Model.Entities;
using R3;
using Utilities.Messages;

namespace Model.Items
{
    public class ItemEntityEvents : IEntityGroupEvents
    {
        private readonly GroupEvents<ItemEntity> _events = new();
        public Observable<(ItemEntity Item, OnPositionChangedMessage Message)> OnPositionChanged => _events.GetObservable<OnPositionChangedMessage>();
        public Observable<(ItemEntity Item, OnDisabledMessage Message)> OnDisabled => _events.GetObservable<OnDisabledMessage>();
        public Observable<(ItemEntity Item, OnMoveMessage Message)> OnMove => _events.GetObservable<OnMoveMessage>();
        public Observable<(ItemEntity Item, OnTeleportMessage Message)> OnTeleport => _events.GetObservable<OnTeleportMessage>();
        public Observable<(ItemEntity Item, OnEffectSpawnedMessage Message)> OnEffectSpawned => _events.GetObservable<OnEffectSpawnedMessage>();

        Observable<(Entity Entity, OnPositionChangedMessage Message)> IEntityGroupEvents.OnPositionChanged => _events.GetSubject<OnPositionChangedMessage>().SelectSender(item => item.Entity);

        Observable<(Entity Entity, OnMoveMessage Message)> IEntityGroupEvents.OnMove => _events.GetSubject<OnMoveMessage>().SelectSender(item => item.Entity);

        Observable<(Entity Entity, OnTeleportMessage Message)> IEntityGroupEvents.OnTeleport => _events.GetSubject<OnTeleportMessage>().SelectSender(item => item.Entity);

        public void Add(ItemEntity item)
        {
            _events.Add(item, item.Position.Select(positionChanged => new OnPositionChangedMessage(positionChanged)));
            _events.Add(item, item.OnDisabled.Select(disabled => new OnDisabledMessage()));
            _events.Add(item, item.OnMove.Select(move => new OnMoveMessage(move.direction, move.destination)));
            _events.Add(item, item.OnSpawnEffect.Select(useSkill => new OnEffectSpawnedMessage(useSkill)));
        }
    }
    public record OnDisabledMessage();
}