#nullable enable
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Character.Type;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.Message;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Model.Character
{
    public interface ICharacter : IDisposable, ISerializable<CharacterMemento>, IEntity, IActor, IHasBehavior,
        IActorOfEffect, ITargetOfEffect, IHasCondition
    {
        public bool IsLeader { get; }
        public bool IsBoss { get; }
        public bool HasHomePosition { get; }
        public CharacterState State { get; }
        public void SetWaitState();
        public int Money { get; }
        public string GetName(IHasAffiliation player, bool ignoreVisibility = false);
        public bool CanAct { get; }
        public bool IsDead { get; }
        public ReadOnlyReactiveProperty<Direction8> Direction { get; }
        public Observable<OnEffectSpawnedMessage> OnEffectSpawned { get; }
        public Observable<Unit> OnDead { get; }
        public Observable<Unit> OnPickUpItem { get; }
        public Observable<OnItemSelectMessage> OnItemSelect { get; }
        public ICharacterType CharacterType { get; init; }
        public IStatusManager StatusManager { get; }
        public Aggression Aggression { get; }
        public IAffiliation Affiliation { get; }
        public Direction8 CurrentDirection { get; }
        public IInventory Inventory { get; }
        public ReadOnlyReactiveProperty<Vector2Int> Position { get; }
        public ReadOnlyReactiveProperty<bool> Visibility { get; }
        public EntityLayer Layer { get; }
        public Observable<(Direction8 direction, Vector2Int destination, bool isThrown)> OnMove { get; }
        public Observable<Vector2Int> OnTeleport { get; }
        public Vector2Int CurrentPosition { get; }
        public ICharacterSkill[] Skills { get; }
        public IVisionRange VisionRange { get; }
        public int CurrentMaxHp { get; }
        public int CurrentHp { get; }
        public bool CanMove(Vector2Int position, Direction8 direction, IPassableChecker map);
        public bool CanMove(Direction8 direction, bool isFlying, IPassableChecker map);
        public bool CanMove(Direction8 direction, IPassableChecker map);
        public bool CanMove(Vector2Int position, Direction8 direction, bool isFlying, IPassableChecker map);
        public bool CanMoveIgnoreEntity(Vector2Int position, Direction8 direction, IPassableChecker map);
        public void Turn(Direction8 direction);
        public void FaceNearestCharacter(IMap map);
        public void DoNothing();
        public UniTask Move(Direction8 direction, IInput input);
        public UniTask UseSkill(ICharacterSkill skill, Direction8 direction, IMap map);
        public UniTask UseItem(IItem item, Direction8 direction, IMap map);
        public UniTask ThrowItem(IItem item, Direction8 direction, IMap map);
        public void SetVisibility(bool visibility);
        public void Teleport(Vector2Int position);
        public int GainHp(int value);
        public int LoseHp(int value);
        public void AddCondition(Id<IEntity> actor, IConditionData condition, RemovalConditionData removalCondition);
        public UniTask ForceMove(Direction8 direction, IInput input);
        public void WasAttackedBy(IActorOfEffect actor, float impact);
        public void WasHealedBy(IActorOfEffect actor, float impact);
        public UniTask DoNextAction(IGameManager gameManager, IMap map, IInput input);
        public bool CanPickUpItem();
        public bool TryPickUp(IItem item);
        public IItem? ReplaceInventory(IItem? item, int index);
        public void UpdateTurn();
        public void AddMoney(int value);
        public void ReduceMoney(int value);

        public bool IsVisible(Vector2Int position)
        {
            return VisionRange.VisibleArea.Contains(position);
        }
    }
}