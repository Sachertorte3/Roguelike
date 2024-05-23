#nullable enable
using Model.Entities;
using R3;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Utilities.Messages;

namespace Model.Characters
{
    public class CharacterEvents : IEntityGroupEvents
    {
        private GroupEvents<Character> _events = new();
        public Observable<(Character Character, OnPositionChangedMessage Message)> OnPositionChanged => _events.GetObservable<OnPositionChangedMessage>();
        public Observable<(Character Character, OnDirectionChangedMessage Message)> OnDirectionChanged => _events.GetObservable<OnDirectionChangedMessage>();
        public Observable<(Character Character, OnDeadMessage Message)> OnDead => _events.GetObservable<OnDeadMessage>();
        public Observable<(Character Character, OnMoveMessage Message)> OnMove => _events.GetObservable<OnMoveMessage>();
        public Observable<(Character Character, OnTeleportMessage Message)> OnTeleport => _events.GetObservable<OnTeleportMessage>();
        public Observable<(Character Character, OnEffectSpawnedMessage Message)> OnEffectSpawned => _events.GetObservable<OnEffectSpawnedMessage>();
        public Observable<(Character Character, OnVisibleAreaChangedMessage Message)> OnVisibleAreaChanged => _events.GetObservable<OnVisibleAreaChangedMessage>();

        Observable<(Entity Entity, OnPositionChangedMessage Message)> IEntityGroupEvents.OnPositionChanged => _events.GetSubject<OnPositionChangedMessage>().SelectSender(character => character.Entity);

        Observable<(Entity Entity, OnMoveMessage Message)> IEntityGroupEvents.OnMove => _events.GetSubject<OnMoveMessage>().SelectSender(character => character.Entity);

        Observable<(Entity Entity, OnTeleportMessage Message)> IEntityGroupEvents.OnTeleport => _events.GetSubject<OnTeleportMessage>().SelectSender(character => character.Entity);

        public void Add(Character character)
        {
            _events.Add(character, character.Position.Select(positionChanged => new OnPositionChangedMessage(positionChanged)));
            _events.Add(character, character.Direction.Select(directionChanged => new OnDirectionChangedMessage(directionChanged)));
            _events.Add(character, character.OnDead.Select(_ => new OnDeadMessage()));
            _events.Add(character, character.OnMove.Select(move => new OnMoveMessage(move.direction, move.destination)));
            _events.Add(character, character.OnTeleport.Select(teleport => new OnTeleportMessage(teleport)));
            _events.Add(character, character.OnSpawnEffect.Select(useSkill =>new OnEffectSpawnedMessage(useSkill)));
            _events.Add(character, character.Area.OnVisibleAreaChanged);
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

    public record OnDirectionChangedMessage(Direction8 Direction);
    public record OnDeadMessage();
    public record OnEffectSpawnedMessage(IEnumerable<Vector2Int> Area);
    public record OnVisibleAreaChangedMessage(HashSet<Vector2Int> NewArea, HashSet<Vector2Int> AreaExited, HashSet<Vector2Int> AreaEntered);
}