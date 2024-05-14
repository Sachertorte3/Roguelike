#nullable enable
using System.Collections.Generic;
using Model.Characters.Effect;
using R3;
using UnityEngine;
using Utilities;

namespace Model.Characters
{
    public class CharacterEvents
    {
        private readonly Subject<OnDeadMessage> _onDead = new();
        private readonly Subject<OnDirectionChangedMessage> _onDirectionChanged = new();
        private readonly Subject<OnMoveMessage> _onMove = new();
        private readonly Subject<OnPositionChangedMessage> _onPositionChanged = new();
        private readonly Subject<OnUseSkillMessage> _onUseSkill = new();
        private readonly Subject<OnVisibleAreaChangedMessage> _onVisibleAreaChanged = new();
        public Observable<OnPositionChangedMessage> OnPositionChanged => _onPositionChanged;
        public Observable<OnDirectionChangedMessage> OnDirectionChanged => _onDirectionChanged;
        public Observable<OnDeadMessage> OnDead => _onDead;
        public Observable<OnMoveMessage> OnMove => _onMove;
        public Observable<OnUseSkillMessage> OnUseSkill => _onUseSkill;
        public Observable<OnVisibleAreaChangedMessage> OnVisibleAreaChanged => _onVisibleAreaChanged;

        public void Add(Character character)
        {
            character.Position.Subscribe(positionChanged =>
                _onPositionChanged.OnNext(new OnPositionChangedMessage(character, positionChanged)));
            character.Direction.Subscribe(directionChanged =>
                _onDirectionChanged.OnNext(new OnDirectionChangedMessage(character, directionChanged)));
            character.OnDead.Subscribe(_ => _onDead.OnNext(new OnDeadMessage(character)));
            character.OnMove.Subscribe(move =>
                _onMove.OnNext(new OnMoveMessage(character, move.direction, move.destination)));
            character.OnUseSkill.Subscribe(useSkill =>
                _onUseSkill.OnNext(new OnUseSkillMessage(character, useSkill.skill, useSkill.position,
                    useSkill.direction)));
            character.Area.OnVisibleAreaChanged.Pairwise().Subscribe(visibleAreaChanged =>
            {
                HashSet<Vector2Int> newArea = new(visibleAreaChanged.Current);
                visibleAreaChanged.Previous.ExceptWith(visibleAreaChanged.Current);
                visibleAreaChanged.Current.ExceptWith(visibleAreaChanged.Previous);
                _onVisibleAreaChanged.OnNext(new OnVisibleAreaChangedMessage(character, newArea,
                    visibleAreaChanged.Previous, visibleAreaChanged.Current));
            });
        }
    }

    public record OnPositionChangedMessage(Character Character, Vector2Int Position);

    public record OnDirectionChangedMessage(Character Character, Direction8 Direction);

    public record OnDeadMessage(Character Character);

    public record OnMoveMessage(Character Character, Direction8 Direction, Vector2Int Destination);

    public record OnUseSkillMessage(Character Character, Skill Skill, Vector2Int Position, Direction8 Direction);

    public record OnVisibleAreaChangedMessage(Character Character, HashSet<Vector2Int> NewArea,
        HashSet<Vector2Int> AreaExited, HashSet<Vector2Int> AreaEntered);
}