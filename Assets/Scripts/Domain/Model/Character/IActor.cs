using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Item;
using UnityEngine;
using Utilities;

namespace Domain.Model.Action
{
    public interface IActor : IActorOfEffect
    {
        public Vector2Int CurrentPosition { get; }
        public Direction8 CurrentDirection { get; }
        public IInventory Inventory { get; }
        public IItemSelecter ItemSelecter { get; }
        public void DoNothing();
        public bool CanMove(Direction8 direction, IPassableChecker world);
        public bool CanMoveIgnoreCharacter(Direction8 direction, IPassableChecker world);
        public UniTask Move(Direction8 direction, IInput input);
        public void Turn(Direction8 direction);
        public UniTask UseSkill(ICharacterSkill skill, Direction8 direction, IMap world);
        public UniTask UseItem(IItem item, Direction8 direction, IMap world);
        public UniTask ThrowItem(IItem item, Direction8 direction, IMap world);
        public float EvaluateThrow(IItem item, Direction8 direction, IMap world);
    }
}