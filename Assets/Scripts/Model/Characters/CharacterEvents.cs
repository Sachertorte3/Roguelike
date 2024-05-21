#nullable enable
using Model.Entities;
using R3;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using Utilities;
using Utilities.Messages;

namespace Model.Characters
{
    public interface IEntityGroupEvents
    {
        public Observable<(Entity Entity, OnPositionChangedMessage Message)> OnPositionChanged { get; }
        public Observable<(Entity Entity, OnMoveMessage Message)> OnMove { get; }
        public Observable<(Entity Entity, OnTeleportMessage Message)> OnTeleport { get; }
    }
    public class CharacterEvents : IEntityGroupEvents
    {
        private readonly MessageSubject<Character, OnDeadMessage> _onDead = new();
        private readonly MessageSubject<Character, OnDirectionChangedMessage> _onDirectionChanged = new();
        private readonly MessageSubject<Character, OnMoveMessage> _onMove = new();
        private readonly MessageSubject<Character, OnTeleportMessage> _onTeleport = new();
        private readonly MessageSubject<Character, OnPositionChangedMessage> _onPositionChanged = new();
        private readonly MessageSubject<Character, OnEffectSpawnedMessage> _onEffectSpawned = new();
        private readonly MessageSubject<Character, OnVisibleAreaChangedMessage> _onVisibleAreaChanged = new();
        public Observable<(Character Character, OnPositionChangedMessage Message)> OnPositionChanged => _onPositionChanged.AsObservable();
        public Observable<(Character Character, OnDirectionChangedMessage Message)> OnDirectionChanged => _onDirectionChanged.AsObservable();
        public Observable<(Character Character, OnDeadMessage Message)> OnDead => _onDead.AsObservable();
        public Observable<(Character Character, OnMoveMessage Message)> OnMove => _onMove.AsObservable();
        public Observable<(Character Character, OnTeleportMessage Message)> OnTeleport => _onTeleport.AsObservable();
        public Observable<(Character Character, OnEffectSpawnedMessage Message)> OnEffectSpawned => _onEffectSpawned.AsObservable();
        public Observable<(Character Character, OnVisibleAreaChangedMessage Message)> OnVisibleAreaChanged => _onVisibleAreaChanged.AsObservable();

        Observable<(Entity Entity, OnPositionChangedMessage Message)> IEntityGroupEvents.OnPositionChanged => _onPositionChanged.AsObservable(character => character.Entity);

        Observable<(Entity Entity, OnMoveMessage Message)> IEntityGroupEvents.OnMove => _onMove.AsObservable(character => character.Entity);

        Observable<(Entity Entity, OnTeleportMessage Message)> IEntityGroupEvents.OnTeleport => _onTeleport.AsObservable(character => character.Entity);

        public Dictionary<object, CompositeDisposable> _disposable = new();

        public void Add(Character character)
        {
            if (_disposable.ContainsKey(character))
            {
                return;
            }
            else
            {
                _disposable[character] = new CompositeDisposable();
            }
            _disposable[character].Add(character.Position.Subscribe(positionChanged =>
                _onPositionChanged.OnNext(character, new OnPositionChangedMessage(positionChanged))));
            _disposable[character].Add(character.Direction.Subscribe(directionChanged =>
                _onDirectionChanged.OnNext(character, new OnDirectionChangedMessage(directionChanged))));
            _disposable[character].Add(character.OnDead.Subscribe(_ => _onDead.OnNext(character, new OnDeadMessage())));
            _disposable[character].Add(character.OnMove.Subscribe(move =>
                _onMove.OnNext(character, new OnMoveMessage(move.direction, move.destination))));
            _disposable[character].Add(character.OnSpawnEffect.Subscribe(useSkill =>
                _onEffectSpawned.OnNext(character, new OnEffectSpawnedMessage(useSkill))));
            _disposable[character].Add(character.Area.OnVisibleAreaChanged.Subscribe(visibleAreaChanged =>
            {
                _onVisibleAreaChanged.OnNext(character, new OnVisibleAreaChangedMessage(visibleAreaChanged.NewArea, visibleAreaChanged.AreaExited, visibleAreaChanged.AreaEntered));
            }));
        }
        public void Add(CharacterEvents characterEvents)
        {
            if (_disposable.ContainsKey(characterEvents))
            {
                return;
            }
            else
            {
                _disposable[characterEvents] = new CompositeDisposable();
            }
            _disposable[characterEvents].Add(characterEvents.OnPositionChanged.RelayTo(_onPositionChanged));
            _disposable[characterEvents].Add(characterEvents.OnDirectionChanged.RelayTo(_onDirectionChanged));
            _disposable[characterEvents].Add(characterEvents.OnDead.RelayTo(_onDead));
            _disposable[characterEvents].Add(characterEvents.OnMove.RelayTo(_onMove));
            _disposable[characterEvents].Add(characterEvents.OnTeleport.RelayTo(_onTeleport));
            _disposable[characterEvents].Add(characterEvents.OnEffectSpawned.RelayTo(_onEffectSpawned));
            _disposable[characterEvents].Add(characterEvents.OnVisibleAreaChanged.RelayTo(_onVisibleAreaChanged));
        }
        public void Remove(Character character)
        {
            _disposable[character].Dispose();
            _disposable.Remove(character);
        }
        public void Remove(CharacterEvents characterEvents)
        {
            _disposable[characterEvents].Dispose();
            _disposable.Remove(characterEvents);
        }
    }

    public record OnPositionChangedMessage(Vector2Int Position);

    public record OnDirectionChangedMessage(Direction8 Direction);

    public record OnDeadMessage();

    public record OnMoveMessage(Direction8 Direction, Vector2Int Destination);

    public record OnTeleportMessage(Vector2Int Position);

    public record OnEffectSpawnedMessage(IEnumerable<Vector2Int> Area);

    public record OnVisibleAreaChangedMessage(HashSet<Vector2Int> NewArea, HashSet<Vector2Int> AreaExited, HashSet<Vector2Int> AreaEntered);
}