#nullable enable
using System;
using Domain.Model.Effect;
using Domain.Service.Entities;
using R3;
using Utilities.Messages;

namespace Domain.Service.Characters
{
    public class CharacterEvents : IDisposable, IEntityGroupEvents
    {
        private GroupEvents<Character> _events = new();

        public Observable<(Character Character, OnPositionChangedMessage Message)> OnPositionChanged =>
            _events.GetObservable<OnPositionChangedMessage>();

        public Observable<(Character Character, OnDirectionChangedMessage Message)> OnDirectionChanged =>
            _events.GetObservable<OnDirectionChangedMessage>();

        public Observable<(Character Character, OnDeadMessage Message)> OnDead =>
            _events.GetObservable<OnDeadMessage>();

        public Observable<(Character Character, OnMoveMessage Message)> OnMove =>
            _events.GetObservable<OnMoveMessage>();

        public Observable<(Character Character, OnTeleportMessage Message)> OnTeleport =>
            _events.GetObservable<OnTeleportMessage>();

        public Observable<(Character Character, OnEffectSpawnedMessage Message)> OnEffectSpawned =>
            _events.GetObservable<OnEffectSpawnedMessage>();

        public Observable<(Character Character, OnVisibleAreaChangedMessage Message)> OnVisibleAreaChanged =>
            _events.GetObservable<OnVisibleAreaChangedMessage>();

        public Observable<(Character Character, OnPickUpItemMessage Message)> OnPickUpItem =>
            _events.GetObservable<OnPickUpItemMessage>();

        public Observable<(Character Character, OnDamageReceivedMessage Message)> OnDamageReceived =>
            _events.GetObservable<OnDamageReceivedMessage>();

        public Observable<(Character Character, OnHealReceivedMessage Message)> OnHealReceived =>
            _events.GetObservable<OnHealReceivedMessage>();

        public Observable<(Character Character, OnAffectionChangedMessage Message)> OnAffectionChanged =>
            _events.GetObservable<OnAffectionChangedMessage>();

        public void Dispose()
        {
            _events.Dispose();
        }

        Observable<(Entity Entity, OnPositionChangedMessage Message)> IEntityGroupEvents.OnPositionChanged =>
            _events.GetSubject<OnPositionChangedMessage>().SelectSender(character => character.Entity);

        Observable<(Entity Entity, OnMoveMessage Message)> IEntityGroupEvents.OnMove =>
            _events.GetSubject<OnMoveMessage>().SelectSender(character => character.Entity);

        Observable<(Entity Entity, OnTeleportMessage Message)> IEntityGroupEvents.OnTeleport =>
            _events.GetSubject<OnTeleportMessage>().SelectSender(character => character.Entity);

        ~CharacterEvents()
        {
            Dispose();
        }

        public void Add(Character character)
        {
            _events.Add(character,
                character.Position.Select(positionChanged => new OnPositionChangedMessage(positionChanged)));
            _events.Add(character,
                character.Direction.Select(directionChanged => new OnDirectionChangedMessage(directionChanged)));
            _events.Add(character, character.OnDead.Select(_ => new OnDeadMessage()));
            _events.Add(character,
                character.OnMove.Select(move => new OnMoveMessage(move.direction, move.destination)));
            _events.Add(character, character.OnTeleport.Select(teleport => new OnTeleportMessage(teleport)));
            _events.Add(character, character.OnEffectSpawned);
            _events.Add(character, character.Area.OnVisibleAreaChanged);
            _events.Add(character, character.OnPickUpItem.Select(_ => new OnPickUpItemMessage()));
            _events.Add(character,
                character.StatusManager.OnDamageReceived.Select(damage => new OnDamageReceivedMessage(damage)));
            _events.Add(character,
                character.StatusManager.OnHealReceived.Select(heal => new OnHealReceivedMessage(heal)));
            _events.Add(character, character.Affiliation.OnAffectionChanged);
        }

        public void Add(CharacterEvents characterEvents)
        {
            _events.Add(characterEvents, characterEvents.OnPositionChanged);
            _events.Add(characterEvents, characterEvents.OnDirectionChanged);
            _events.Add(characterEvents, characterEvents.OnDead);
            _events.Add(characterEvents, characterEvents.OnMove);
            _events.Add(characterEvents, characterEvents.OnTeleport);
            _events.Add(characterEvents, characterEvents.OnEffectSpawned);
            _events.Add(characterEvents, characterEvents.OnVisibleAreaChanged);
            _events.Add(characterEvents, characterEvents.OnPickUpItem);
            _events.Add(characterEvents, characterEvents.OnDamageReceived);
            _events.Add(characterEvents, characterEvents.OnHealReceived);
            _events.Add(characterEvents, characterEvents.OnAffectionChanged);
        }

        public void Remove(Character character)
        {
            _events.Remove(character);
        }

        public void Remove(CharacterEvents characterEvents)
        {
            _events.Remove(characterEvents);
        }
    }
}