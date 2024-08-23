using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Service.Events;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Rooms
{
    public class Clerk : IEventEntity
    {
        public readonly ICharacter Character;
        private readonly Func<bool> _canExecuteEvent;
        public bool CanExecuteEvent => _canExecuteEvent();
        private readonly Func<IMapManager, UniTask> _doEvent;

        public Clerk(ICharacter character, Func<bool> canExecuteEvent, Func<IMapManager, UniTask> doEvent)
        {
            Character = character;
            _canExecuteEvent = canExecuteEvent;
            _doEvent = doEvent;
        }
        public void Dispose()
        {
            Character.Dispose();
        }
        ~Clerk()
        {
            Dispose();
        }
        public EventTrigger Trigger => EventTrigger.Touch;
        public Id<IEntity> Id => Character.Id;
        public ReadOnlyReactiveProperty<Vector2Int> Position => Character.Position;
        public Vector2Int CurrentPosition => Character.CurrentPosition;
        public ReadOnlyReactiveProperty<bool> Visibility => Character.Visibility;
        public EntityLayer Layer => Character.Layer;
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => Character.OnMove;
        public Observable<Vector2Int> OnTeleport => Character.OnTeleport;
        public Observable<Unit> OnDestroyed => Character.OnDestroyed;

        public async UniTask DoEvent(IGameManager gameManager, IMapManager mapManager)
        {
            await _doEvent(mapManager);
        }

        public void ReducesFavorabilityTowardsThief(ICharacter thief)
        {
            Character.Affiliation.OnCharacterAttacked(thief.Affiliation, Character.Affiliation, 1f);
        }

        public void SetVisibility(bool visibility)
        {
            Character.SetVisibility(visibility);
        }

        public void Destroy()
        {
            Character.Destroy();
        }
    }
}