#nullable enable
using System;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Message;
using Domain.Service.Entities;
using R3;
using Utilities.Messages;

namespace Domain.Service.Characters
{
    public class CharacterEvents : IDisposable, IEntityGroupEvents
    {
        private GroupEvents<ICharacter> _events = new();

        public Observable<(ICharacter Character, OnPositionChangedMessage Message)> OnPositionChanged =>
            _events.GetObservable<OnPositionChangedMessage>();

        public Observable<(ICharacter Character, OnDirectionChangedMessage Message)> OnDirectionChanged =>
            _events.GetObservable<OnDirectionChangedMessage>();

        public Observable<(ICharacter Character, OnDeadMessage Message)> OnDead =>
            _events.GetObservable<OnDeadMessage>();

        public Observable<(ICharacter Character, OnMoveMessage Message)> OnMove =>
            _events.GetObservable<OnMoveMessage>();

        public Observable<(ICharacter Character, OnTeleportMessage Message)> OnTeleport =>
            _events.GetObservable<OnTeleportMessage>();

        public Observable<(ICharacter Character, OnDestroyedMessage Message)> OnDestroyed =>
            _events.GetObservable<OnDestroyedMessage>();

        public Observable<(ICharacter Character, Unit Message)> OnVisibleAreaChanged =>
            _events.GetObservable<Unit>();

        public Observable<(ICharacter Character, OnPickUpItemMessage Message)> OnPickUpItem =>
            _events.GetObservable<OnPickUpItemMessage>();

        public Observable<(ICharacter Character, OnDamageReceivedMessage Message)> OnDamageReceived =>
            _events.GetObservable<OnDamageReceivedMessage>();

        public Observable<(ICharacter Character, OnHealReceivedMessage Message)> OnHealReceived =>
            _events.GetObservable<OnHealReceivedMessage>();

        public Observable<(ICharacter Character, OnAffiliationChangedMessage Message)> OnAffiliationChanged =>
            _events.GetObservable<OnAffiliationChangedMessage>();

        public Observable<(ICharacter Character, OnAffectedByTrapFlagsChangedMessage Message)> OnAffectedByTrapFlagsChanged =>
            _events.GetObservable<OnAffectedByTrapFlagsChangedMessage>();

        public void Dispose()
        {
            _events.Dispose();
        }

        Observable<(IEntity Entity, OnPositionChangedMessage Message)> IEntityGroupEvents.OnPositionChanged =>
            _events.GetSubject<OnPositionChangedMessage>().SelectSender(character => (IEntity)character);

        Observable<(IEntity Entity, OnMoveMessage Message)> IEntityGroupEvents.OnMove =>
            _events.GetSubject<OnMoveMessage>().SelectSender(character => (IEntity)character);

        Observable<(IEntity Entity, OnTeleportMessage Message)> IEntityGroupEvents.OnTeleport =>
            _events.GetSubject<OnTeleportMessage>().SelectSender(character => (IEntity)character);

        Observable<(IEntity Entity, OnDestroyedMessage Message)> IEntityGroupEvents.OnDestroyed =>
            _events.GetSubject<OnDestroyedMessage>().SelectSender(character => (IEntity)character);

        ~CharacterEvents()
        {
            Dispose();
        }

        public void Add(ICharacter character)
        {
            _events.Add(character,
                character.Position.Select(positionChanged => new OnPositionChangedMessage(positionChanged)));
            _events.Add(character,
                character.Direction.Select(directionChanged => new OnDirectionChangedMessage(directionChanged)));
            _events.Add(character, character.OnDead.Select(_ => new OnDeadMessage()));
            _events.Add(character,
                character.OnMove.Select(move => new OnMoveMessage(move.direction, move.destination)));
            _events.Add(character, character.OnTeleport.Select(teleport => new OnTeleportMessage(teleport)));
            _events.Add(character, character.OnDestroyed.Select(_ => new OnDestroyedMessage()));
            _events.Add(character, character.VisionRange.OnVisibleAreaChanged);
            _events.Add(character, character.OnPickUpItem.Select(_ => new OnPickUpItemMessage()));
            _events.Add(character,
                character.StatusManager.OnDamageReceived.Select(damage => new OnDamageReceivedMessage(damage)));
            _events.Add(character,
                character.StatusManager.OnHealReceived.Select(heal => new OnHealReceivedMessage(heal)));
            _events.Add(character, character.Affiliation.OnAffiliationChanged);
            _events.Add(character,
                character.StatusManager.IsAffectedByTraps.Select(isAffectedByTrap => new OnAffectedByTrapFlagsChangedMessage(isAffectedByTrap)));
        }

        public void Remove(ICharacter character)
        {
            _events.Remove(character);
        }
    }
}