#nullable enable
using Model.Characters;
using Model.Entities;
using R3;
using Utilities.Messages;

namespace Model.Items
{
    public class ItemEntityEvents : IEntityGroupEvents
    {
        private readonly MessageSubject<ItemEntity, OnDisabledMessage> _onDisabled = new();
        private readonly MessageSubject<ItemEntity, OnMoveMessage> _onMove = new();
        private readonly MessageSubject<ItemEntity, OnTeleportMessage> _onTeleport = new();
        private readonly MessageSubject<ItemEntity, OnPositionChangedMessage> _onPositionChanged = new();
        private readonly MessageSubject<ItemEntity, OnEffectSpawnedMessage> _onEffectSpawned = new();
        public Observable<(ItemEntity Item, OnPositionChangedMessage Message)> OnPositionChanged => _onPositionChanged;
        public Observable<(ItemEntity Item, OnDisabledMessage Message)> OnDisabled => _onDisabled;
        public Observable<(ItemEntity Item, OnMoveMessage Message)> OnMove => _onMove;
        public Observable<(ItemEntity Item, OnTeleportMessage Message)> OnTeleport => _onTeleport;
        public Observable<(ItemEntity Item, OnEffectSpawnedMessage Message)> OnEffectSpawned => _onEffectSpawned;

        Observable<(Entity Entity, OnPositionChangedMessage Message)> IEntityGroupEvents.OnPositionChanged => _onPositionChanged.SelectSender(item => item.Entity);

        Observable<(Entity Entity, OnMoveMessage Message)> IEntityGroupEvents.OnMove => _onMove.SelectSender(item => item.Entity);

        Observable<(Entity Entity, OnTeleportMessage Message)> IEntityGroupEvents.OnTeleport => _onTeleport.SelectSender(item => item.Entity);

        public void Add(ItemEntity item)
        {
            item.Position.Subscribe(positionChanged =>
                _onPositionChanged.OnNext(item, new OnPositionChangedMessage(positionChanged)));
            item.OnDisabled.Subscribe(disabled => _onDisabled.OnNext(item, new OnDisabledMessage()));
            item.OnMove.Subscribe(move => _onMove.OnNext(item, new OnMoveMessage(move.direction, move.destination)));
            item.OnSpawnEffect.Subscribe(useSkill =>
                _onEffectSpawned.OnNext(item, new OnEffectSpawnedMessage(useSkill)));
        }
    }
    public record OnDisabledMessage();
}