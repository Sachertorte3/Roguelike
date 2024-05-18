#nullable enable
using System.Collections.Generic;
using Model.Effect;
using R3;
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

        public void Add(Character character)
        {
            character.Position.Subscribe(positionChanged =>
                _onPositionChanged.OnNext(new OnCharacterPositionChangedMessage(character, positionChanged)));
            character.Direction.Subscribe(directionChanged =>
                _onDirectionChanged.OnNext(new OnCharacterDirectionChangedMessage(character, directionChanged)));
            character.OnDead.Subscribe(_ => _onDead.OnNext(new OnCharacterDeadMessage(character)));
            character.OnMove.Subscribe(move =>
                _onMove.OnNext(new OnCharacterMoveMessage(character, move.direction, move.destination)));
            character.OnSpawnEffect.Subscribe(useSkill =>
                _onUseSkill.OnNext(new OnCharacterSpawnEffectMessage(character, useSkill)));
            character.Area.OnVisibleAreaChanged.Subscribe(visibleAreaChanged => _onVisibleAreaChanged.OnNext(new OnCharacterVisibleAreaChangedMessage(character, visibleAreaChanged.NewArea, visibleAreaChanged.AreaExited, visibleAreaChanged.AreaEntered)));
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