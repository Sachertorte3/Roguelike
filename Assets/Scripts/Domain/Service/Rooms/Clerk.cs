#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Rooms
{
    public class Clerk : IEventEntity
    {
        public readonly ICharacter Character;
        public string? ChoiceMessage => null;
        private readonly List<EntityEvent> _events;
        public IReadOnlyList<EntityEvent> Events => _events;
        public bool CanBeCanceled => true;

        public Clerk(ICharacter character, Func<bool> canExecuteEvent, Func<IGameManager, IMap, UniTask> doEvent)
        {
            Character = character;
            _events = new()
            {
                new EntityEvent("代金を支払う", canExecuteEvent, doEvent)
            };
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
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => Character.OnMove;
        public Observable<Vector2Int> OnTeleport => Character.OnTeleport;
        public Observable<Unit> OnDestroyed => Character.OnDestroyed;

        public void ReducesFavorabilityTowardsThief(ICharacter thief)
        {
            Character.Affiliation.ForceAffiliation(thief.Affiliation, AffiliationType.Enemy);
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