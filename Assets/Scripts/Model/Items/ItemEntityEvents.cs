#nullable enable
using Model.Characters.Effect;
using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Model.Items
{
    public class ItemEntityEvents
    {
        private readonly Subject<OnDisabledMessage> _onDisabled = new();
        private readonly Subject<OnMoveMessage> _onMove = new();
        private readonly Subject<OnPositionChangedMessage> _onPositionChanged = new();
        private readonly Subject<OnUseSkillMessage> _onUseSkill = new();
        public Observable<OnPositionChangedMessage> OnPositionChanged => _onPositionChanged;
        public Observable<OnDisabledMessage> OnDisabled => _onDisabled;
        public Observable<OnMoveMessage> OnMove => _onMove;
        public Observable<OnUseSkillMessage> OnUseSkill => _onUseSkill;

        public void Add(ItemEntity item)
        {
            item.Position.Subscribe(positionChanged =>
                _onPositionChanged.OnNext(new OnPositionChangedMessage(item, positionChanged)));
            item.OnDisabled.Subscribe(disabled => _onDisabled.OnNext(new OnDisabledMessage(item)));
            item.OnMove.Subscribe(move => _onMove.OnNext(new OnMoveMessage(item, move.direction, move.destination)));
            item.OnSpawnEffect.Subscribe(useSkill =>
                _onUseSkill.OnNext(new OnUseSkillMessage(item, useSkill)));
        }
    }

    public record OnMoveMessage(ItemEntity Item, Direction8 Direction, Vector2Int Destination);

    public record OnDisabledMessage(ItemEntity Item);

    public record OnPositionChangedMessage(ItemEntity Item, Vector2Int Position);

    public record OnUseSkillMessage(ItemEntity Item, IEnumerable<Vector2Int> Area);
}