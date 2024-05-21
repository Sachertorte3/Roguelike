#nullable enable
using R3;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Model.Characters
{
    public class CharacterEvents
    {
        private readonly Subject<OnCharacterDeadMessage> _onDead = new();
        private readonly Subject<OnCharacterDirectionChangedMessage> _onDirectionChanged = new();
        private readonly Subject<OnCharacterMoveMessage> _onMove = new();
        private readonly Subject<OnCharacterPositionChangedMessage> _onPositionChanged = new();
        private readonly Subject<OnCharacterSpawnEffectMessage> _onUseSkill = new();
        private readonly Subject<OnCharacterVisibleAreaChangedMessage> _onVisibleAreaChanged = new();
        public Observable<OnCharacterPositionChangedMessage> OnPositionChanged => _onPositionChanged;
        public Observable<OnCharacterDirectionChangedMessage> OnDirectionChanged => _onDirectionChanged;
        public Observable<OnCharacterDeadMessage> OnDead => _onDead;
        public Observable<OnCharacterMoveMessage> OnMove => _onMove;
        public Observable<OnCharacterSpawnEffectMessage> OnUseSkill => _onUseSkill;
        public Observable<OnCharacterVisibleAreaChangedMessage> OnVisibleAreaChanged => _onVisibleAreaChanged;
        public Dictionary<object, CompositeDisposable> _disposable = new();

        public void Add(Character character)
        {
            if (_disposable.ContainsKey(character))
            {
                _disposable[character].Dispose();
            }
            else
            {
                _disposable[character] = new CompositeDisposable();
            }    
            _disposable[character].Add(character.Position.Subscribe(positionChanged =>
                _onPositionChanged.OnNext(new OnCharacterPositionChangedMessage(character, positionChanged))));
            _disposable[character].Add(character.Direction.Subscribe(directionChanged =>
                _onDirectionChanged.OnNext(new OnCharacterDirectionChangedMessage(character, directionChanged))));
            _disposable[character].Add(character.OnDead.Subscribe(_ => _onDead.OnNext(new OnCharacterDeadMessage(character))));
            _disposable[character].Add(character.OnMove.Subscribe(move =>
                _onMove.OnNext(new OnCharacterMoveMessage(character, move.direction, move.destination))));
            _disposable[character].Add(character.OnSpawnEffect.Subscribe(useSkill =>
                _onUseSkill.OnNext(new OnCharacterSpawnEffectMessage(character, useSkill))));
            _disposable[character].Add(character.Area.OnVisibleAreaChanged.Subscribe(visibleAreaChanged =>
            {
                _onVisibleAreaChanged.OnNext(new OnCharacterVisibleAreaChangedMessage(character, visibleAreaChanged.NewArea, visibleAreaChanged.AreaExited, visibleAreaChanged.AreaEntered));
            }));
        }
        public void Add(CharacterEvents characterEvents)
        {
            if (_disposable.ContainsKey(characterEvents))
            {
                _disposable[characterEvents].Dispose();
            }
            else
            {
                _disposable[characterEvents] = new CompositeDisposable();
            }
            _disposable[characterEvents].Add(characterEvents.OnPositionChanged.Subscribe(positionChanged =>
                _onPositionChanged.OnNext(positionChanged)));
            _disposable[characterEvents].Add(characterEvents.OnDirectionChanged.Subscribe(positionChanged =>
                _onDirectionChanged.OnNext(positionChanged)));
            _disposable[characterEvents].Add(characterEvents.OnDead.Subscribe(positionChanged =>
                _onDead.OnNext(positionChanged)));
            _disposable[characterEvents].Add(characterEvents.OnMove.Subscribe(positionChanged =>
                _onMove.OnNext(positionChanged)));
            _disposable[characterEvents].Add(characterEvents.OnUseSkill.Subscribe(positionChanged =>
                _onUseSkill.OnNext(positionChanged)));
            _disposable[characterEvents].Add(characterEvents.OnVisibleAreaChanged.Subscribe(positionChanged =>
                _onVisibleAreaChanged.OnNext(positionChanged)));
        }
        public void Remove(Character character)
        {
            _disposable[character].Dispose();
        }
        public void Remove(CharacterEvents characterEvents)
        {
            _disposable[characterEvents].Dispose();
        }
    }

    public record OnCharacterPositionChangedMessage(Character Character, Vector2Int Position);

    public record OnCharacterDirectionChangedMessage(Character Character, Direction8 Direction);

    public record OnCharacterDeadMessage(Character Character);

    public record OnCharacterMoveMessage(Character Character, Direction8 Direction, Vector2Int Destination);

    public record OnCharacterSpawnEffectMessage(Character Character, IEnumerable<Vector2Int> Area);

    public record OnCharacterVisibleAreaChangedMessage(Character Character, HashSet<Vector2Int> NewArea,
        HashSet<Vector2Int> AreaExited, HashSet<Vector2Int> AreaEntered);
}