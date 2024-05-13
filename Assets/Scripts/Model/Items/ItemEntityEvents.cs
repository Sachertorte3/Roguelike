#nullable enable
using R3;
using Scripts.Model.Characters;
using Scripts.Model.Characters.Effect;
using Scripts.Utilities;
using UnityEngine;

namespace Scripts.Model.Items
{
    public class ItemEntityEvents
    {
        public Observable<OnPositionChangedMessage> OnPositionChanged => _onPositionChanged;
        private readonly Subject<OnPositionChangedMessage> _onPositionChanged = new();
        public Observable<OnDisabledMessage> OnDisabled => _onDisabled;
        private readonly Subject<OnDisabledMessage> _onDisabled = new();
        public Observable<OnMoveMessage> OnMove => _onMove;
        private readonly Subject<OnMoveMessage> _onMove = new();
        public Observable<OnUseSkillMessage> OnUseSkill => _onUseSkill;
        private readonly Subject<OnUseSkillMessage> _onUseSkill = new();
        public void Add(ItemEntity item)
        {
            item.Position.Subscribe(positionChanged => _onPositionChanged.OnNext(new OnPositionChangedMessage(item, positionChanged)));
            item.OnDisabled.Subscribe(disabled => _onDisabled.OnNext(new OnDisabledMessage(item)));
            item.OnMove.Subscribe(move => _onMove.OnNext(new OnMoveMessage(item, move.direction, move.destination)));
            item.OnUseSkill.Subscribe(useSkill => _onUseSkill.OnNext(new OnUseSkillMessage(item, useSkill.skill, useSkill.position, useSkill.direction)));
        }
    }
    public record OnMoveMessage(ItemEntity Item, Direction8 Direction, Vector2Int Destination);
    public record OnDisabledMessage(ItemEntity Item);
    public record OnPositionChangedMessage(ItemEntity Item, Vector2Int Position);
    public record OnUseSkillMessage(ItemEntity Item, Skill Skill, Vector2Int Position, Direction8 Direction);
}
