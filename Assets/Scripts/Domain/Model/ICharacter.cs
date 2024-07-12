#nullable enable
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Character.Type;
using Domain.Model.Characters;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Items;
using Domain.Model.Message;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Model
{
    public interface ICharacter : IDisposable, ISerializable<CharacterMemento>, IEntity, IActor, IHasBehavior,
        IActorOfEffect, ITargetOfEffect
    {
        public bool IsLeader { get; }
        public bool IsBoss { get; }
        public CharacterState State { get; }
        public int Money { get; }
        public string GetName(IHasAffiliation player);
        public bool CanAct { get; }
        public ReadOnlyReactiveProperty<Direction8> Direction { get; }
        public Observable<OnEffectSpawnedMessage> OnEffectSpawned { get; }
        public Observable<Unit> OnDead { get; }
        public Observable<Unit> OnPickUpItem { get; }
        public ICharacterType CharacterType { get; init; }
        public IStatusManager StatusManager { get; }
        public Aggression Aggression { get; }
        public IAffiliation Affiliation { get; }
        public Direction8 CurrentDirection { get; }
        public IInventory Inventory { get; }
        public ReadOnlyReactiveProperty<Vector2Int> Position { get; }
        public ReadOnlyReactiveProperty<bool> Visibility { get; }
        public EntityLayer Layer { get; }
        public Observable<(Direction8 direction, Vector2Int destination)> OnMove { get; }
        public Observable<Vector2Int> OnTeleport { get; }
        public Vector2Int CurrentPosition { get; }
        public ISkill[] Skills { get; }

        public IVisionRange Area { get; }
        public int CurrentMaxHp { get; }
        public int CurrentHp { get; }

        public bool CanMove(Direction8 direction, IPassableChecker world);
        public bool CanMoveIgnoreCharacter(Direction8 direction, IPassableChecker world);
        public void Turn(Direction8 direction);
        public void DoNothing();
        public UniTask Move(Direction8 direction, IInput input);
        public UniTask UseSkill(ISkill skill, Direction8 direction, IMap map);
        public UniTask UseItem(int itemIndex, Direction8 direction, IMap map);
        public UniTask ThrowItem(int itemIndex, Direction8 direction, IMap world);
        public UniTask<int> GainHp(int value);

        public void SetVisiblity(bool visiblity);
        public UniTask BlowAway(Direction8 direction, int distance, IPassableChecker map);
        public void Teleport(Vector2Int position);
        public UniTask<int> LoseHp(int value);
        public void AddCondition(IConditionData condition, RemovalConditionData removalCondition);
        public UniTask ForceMove(Direction8 direction, IInput input);
        public void WasAttackedBy(IActorOfEffect actor, float impact);
        public void WasHealedBy(IActorOfEffect actor, float impact);
        public UniTask DoNextAction(IMap world, IInput input);
        public bool TryPickUp(IItem item);
        public IItem? ReplaceInventory(IItem? item, int index);
        public void RepairAllItem();
        public void UpdateTurn(IMap world);
        public void AddMoney(int value);
        public void ReduceMoney(int value);

        public bool IsVisible(Vector2Int position)
        {
            return Area.VisibleArea.Contains(position);
        }
    }
}