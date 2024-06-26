using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Service.Effect;
using Domain.Service.Items;
using UnityEngine;
using Utilities;

namespace Domain.Service.Action
{
    public interface IActor : IActorOfEffect
    {
        public Vector2Int CurrentPosition { get; }
        public Direction8 CurrentDirection { get; }
        public IInventory Inventory { get; }
        public void DoNothing();
        public bool CanMove(Direction8 direction, IPassableChecker world);
        public bool CanMoveIgnoreCharacter(Direction8 direction, IPassableChecker world);
        public UniTask Move(Direction8 direction, IInput input);
        public void Turn(Direction8 direction);
        public UniTask UseSkill(Skill skill, Direction8 direction, IMap world);
        public UniTask UseItem(int itemIndex, Direction8 direction, IMap world);
        public UniTask ThrowItem(int itemIndex, Direction8 direction, IMap world);
    }
}