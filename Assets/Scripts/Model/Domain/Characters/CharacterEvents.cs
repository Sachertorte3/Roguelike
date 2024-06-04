#nullable enable
using System;
using Data.Effect;
using Model.Domain.Entities;
using R3;
using Utilities.Messages;

namespace Model.Domain.Characters
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

        public Observable<(Character Character, OnAffectionChangedMessage Message)> OnAffectionChanged =>
            _events.GetObservable<OnAffectionChangedMessage>();

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
        public void Dispose()
        {
            _events.Dispose();
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
            _events.Add(character, character.OnSpawnEffect.Select(useSkill => new OnEffectSpawnedMessage(useSkill)));
            _events.Add(character, character.Area.OnVisibleAreaChanged);
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