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
    public class Clerk : IDisposable, IEventEntity
    {
        private readonly Subject<Unit> _onEventDone = new();
        public readonly ICharacter Character;

        public Clerk(ICharacter character)
        {
            Character = character;
        }

        public Observable<Unit> OnEventDone => _onEventDone;

        public void Dispose()
        {
            _onEventDone.Dispose();
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
            _onEventDone.OnNext(Unit.Default);
        }

        public void SetVisiblity(bool visiblity)
        {
            Character.SetVisiblity(visiblity);
        }
    }
}