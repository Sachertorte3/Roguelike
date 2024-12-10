using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Stairs : IDisposable, ISerializable<StairsMemento>, IPlayerEventEntity, IMovementEntity
    {
        public MovementEntityType Type { get; init; }
        public Location Destination { get; init; }
        public EntityBase Entity { get; init; }
        public Id<IEntity> DestinationId { get; init; }
        public ReadOnlyReactiveProperty<bool> IsLocked { get; private set; }

        public Stairs(StairsMemento data, ReadOnlyReactiveProperty<bool> isLocked)
        {
            Type = data.Type;
            Entity = new EntityBase(data.Entity);
            Destination = data.Destination;
            DestinationId = data.DestinationId;
            IsLocked = isLocked;
            Event = new PlayerEvent(
                "階段を見つけた",
                true,
                new List<PlayerChoiceEvent>
                {
                    new(
                        "進む",
                        player => CanExecuteEvent(),
                        (gameManager, map) => DoEvent(gameManager)
                    )
                }
            );
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public IPlayerEvent Event { get; init; }

        private bool CanExecuteEvent()
        {
            return !IsLocked.CurrentValue;
        }

        private UniTask DoEvent(IGameManager gameManager)
        {
            gameManager.MoveMap(Destination, DestinationId);
            return UniTask.CompletedTask;
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public StairsMemento Serialize()
        {
            return new StairsMemento
            (
                Type,
                Destination,
                entity: Entity.Serialize(),
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
                entity: EntityBase.Build(id, position, EntityLayer.Bottom),
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