#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Service.Events;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Rooms
{
    public class Clerk : IEventEntity
    {
        public readonly ICharacter Character;
        public IEvent Event { get; init; }

        public Clerk(ICharacter character, Func<ICharacter, bool> canExecuteEvent, Func<IGameManager, IMap, UniTask> doEvent)
        {
            Character = character;
            Event = new PlayerEvent(
                null,
                true,
                new List<PlayerChoiceEvent>
                {
                    new PlayerChoiceEvent(
                        "代金を支払う",
                        canExecuteEvent,
                        (player, gameManager, map) => doEvent(gameManager, map)
                    )
                }
            );
        }

        public void Dispose()
        {
            Character.Dispose();
        }

        ~Clerk()
        {
            Dispose();
        }

        public Id<IEntity> Id => Character.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => Character.Position;
        public Vector2Int CurrentPosition => Character.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => Character.Visibility;
        public EntityLayer Layer => Character.Layer;
        public Observable<(Direction8 direction, Vector2Int destination, bool isThrown)> OnMove => Character.OnMove;
        public Observable<Vector2Int> OnTeleport => Character.OnTeleport;
        public Observable<Unit> OnDestroyed => Character.OnDestroyed;

        public void OpposingThief(ICharacter thief)
        {
            Character.Affiliation.AddForceAffiliation(thief.Id, AffiliationType.Enemy);
        }

        public void SetVisibility(bool visibility)
        {
            Character.SetVisibility(visibility);
        }

        public void Destroy()
        {
            Character.Destroy();
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return Character.BlowAway(actor, direction, distance, map);
        }

        public void Teleport(Vector2Int position)
        {
            Character.Teleport(position);
        }
    }
}