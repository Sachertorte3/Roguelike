using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Entities;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace Domain.Service.Events
{
    public class Stairs : IDisposable, ISerializable<StairsMemento>, IIconEventEntity, IMovementEntity
    {
        public MovementEntityType Type { get; init; }
        public Location Destination { get; init; }
        private readonly Entity _entity;
        public Id<IEntity> DestinationId { get; init; }
        public ReadOnlyReactiveProperty<bool> IsLocked { get; private set; }

        public Stairs(StairsMemento data, ReadOnlyReactiveProperty<bool> isLocked)
        {
            Type = data.Type;
            _entity = new Entity(data.Entity);
            Destination = data.Destination;
            DestinationId = data.DestinationId;
            IsLocked = isLocked;
            Event = new PlayerEvent(
                "階段を見つけた",
                true,
                new List<PlayerChoiceEvent>
                {
                    new PlayerChoiceEvent(
                        "進む",
                        (player) => CanExecuteEvent(),
                        (player, gameManager, map) => DoEvent(gameManager)
                    )
                }
            );
        }

        public void Dispose()
        {
            _entity.Dispose();
        }

        public Id<IEntity> Id => _entity.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _entity.Position;
        public ReadOnlyReactiveProperty<bool> Visibility => _entity.VisibleByPlayer;
        public EntityLayer Layer => _entity.Layer;
        public Observable<(Direction8 direction, Vector2Int destination, bool isThrown)> OnMove => _entity.OnMove;
        public Observable<Vector2Int> OnTeleport => _entity.OnTeleport;
        public Observable<Unit> OnDestroyed => _entity.OnDestroyed;

        public Sprite Icon => Type switch
        {
            MovementEntityType.UpStairs => Addressables
                .LoadAssetAsync<Sprite>("MapChip/(Base)BaseChip_pipo.png[(Base)BaseChip_pipo_342]").WaitForCompletion(),
            MovementEntityType.DownStairs => Addressables
                .LoadAssetAsync<Sprite>("MapChip/(Base)BaseChip_pipo.png[(Base)BaseChip_pipo_334]").WaitForCompletion(),
            _ => throw new NotImplementedException()
        };

        public IEvent Event { get; init; }

        private bool CanExecuteEvent()
        {
            return !IsLocked.CurrentValue;
        }

        private UniTask DoEvent(IGameManager gameManager)
        {
            gameManager.LoadMap(Destination, DestinationId);
            return UniTask.CompletedTask;
        }

        public void SetVisibility(bool visibility)
        {
            _entity.SetVisibility(visibility);
        }

        public void Destroy()
        {
            _entity.Destroy();
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public void Teleport(Vector2Int position)
        {
            _entity.Teleport(position);
        }

        public StairsMemento Serialize()
        {
            return new StairsMemento
            (
                Type,
                Destination,
                entity: _entity.Serialize(),
                destinationId: DestinationId
            );
        }

        public static StairsMemento Build(MovementEntityType type, Vector2Int position, Id<IEntity> id,
            Location destination, Id<IEntity> destinationId)
        {
            return new StairsMemento
            (
                type,
                destination,
                entity: Entity.Build(id, position, EntityLayer.Bottom),
                destinationId: destinationId
            );
        }

        public static StairsMemento Build(MovementEntityType type, Vector2Int position, Location destination)
        {
            return Build(type, position, Id<IEntity>.Generate(), destination, Id<IEntity>.Generate());
        }

        ~Stairs()
        {
            Dispose();
        }
    }
}