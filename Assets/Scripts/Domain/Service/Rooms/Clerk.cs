using System;
using Domain.Model;
using Domain.Service.Characters;
using Domain.Service.Events;
using Model.Game;
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
        private readonly Action<IMapManager> _doEvent;

        public Clerk(ICharacter character, Func<bool> canExecuteEvent, Action<IMapManager> doEvent)
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

        public ReadOnlyReactiveProperty<Vector2Int> Position => Character.Position;

        public Vector2Int CurrentPosition => Character.CurrentPosition;

        public ReadOnlyReactiveProperty<bool> Visibility => Character.Visibility;

        public EntityLayer Layer => Character.Layer;

        public Observable<(Direction8 direction, Vector2Int destination)> OnMove => Character.OnMove;

        public Observable<Vector2Int> OnTeleport => Character.OnTeleport;

        public void DoEvent(IGameManager gameManager, IMapManager mapManager)
        {
            _doEvent(mapManager);
        }

        public void ReducesFavorabilityTowardsThief(ICharacter thief)
        {
            Character.Affiliation.OnCharacterAttacked(thief.Affiliation, Character.Affiliation, 1f);
        }

        public void SetVisiblity(bool visiblity)
        {
            Character.SetVisiblity(visiblity);
        }
    }
}