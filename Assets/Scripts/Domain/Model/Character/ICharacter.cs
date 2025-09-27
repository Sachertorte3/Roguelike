#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character.Message;
using Domain.Model.Character.Type;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Result;

namespace Domain.Model.Character
{
    public interface ICharacter : IDisposable, ISerializable<CharacterMemento>, IHasInfo, IEntity, IHasBehavior,
        IHasCondition, IPlayerEventEntity
    {
        public ICharacterType CharacterType { get; init; }
        public bool IsPlayer { get; }
        public bool IsLeader { get; }
        public bool IsBoss { get; }
        public CharacterState State { get; }
        public void SetWaitState();
        public bool IsDead { get; }
        public ReadOnlyReactiveProperty<Direction8> Direction { get; }
        public ReadOnlyReactiveProperty<bool> HasEvent { get; }
        public Observable<Unit> OnAttacked { get; }
        public Observable<Unit> OnDead { get; }
        public Observable<Unit> OnPickUpItem { get; }
        public Observable<OnItemSelectMessage> OnItemSelect { get; }
        public IObservableCollection<string> KnownItemNames { get; }
        public Observable<OnChargeActionUpdatedMessage> OnChargeActionUpdated { get; }
        public bool CanMove(Vector2Int position, Direction8 direction, bool isFlying, bool canThroughWalls,
            IPassableChecker map);

        public bool CanMoveIgnoreEntity(Vector2Int position, Direction8 direction, IPassableChecker map);
        public void FaceNearestCharacter(IMap map);
        public UniTask ForceMove(Direction8 direction, IInput input);
        public void OnAttackedBy(IActorOfEffect actor, float impact);
        public void OnHealedBy(IActorOfEffect actor, float impact);
        public UniTask DoNextAction(IGameManager gameManager, IMap map, IInput input);
        public void CancelChargeAction();
        public bool CanPickUpItem();
        public bool TryAddToInventory(IItem item);
        public IItem? RemoveInventory(ItemFocus index);
        public IEnumerable<IItem> ClearInventory();
        public Result<IItem?> ReplaceInventory(IItem? item, ItemFocus index);
        public void AddEvent(IPlayerEvent ev);
        public void UpdateTurn();

        public bool IsVisible(Vector2Int position)
        {
            return VisionRange.IsVisible(position);
        }
    }
}