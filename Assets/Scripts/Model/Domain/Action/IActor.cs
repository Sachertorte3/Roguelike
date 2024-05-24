using Cysharp.Threading.Tasks;
using Data;
using Model.Domain;
using Model.Effect;
using Model.Items;
using UnityEngine;
using Utilities;

namespace Model.Action
{
    public interface IActor : IActorOfEffect
    {
        public Vector2Int CurrentPosition { get; }
        public Direction8 CurrentDirection { get; }
        public IInventory Inventory { get; }
        public bool CanMove(Direction8 direction, IWorld world);
        public UniTask Move(Direction8 direction, IWorld world);
        public void Turn(Direction8 direction);
        public UniTask UseSkill(Skill skill, Direction8 direction, IWorld world);
        public UniTask UseItem(int itemIndex, Direction8 direction, IWorld world);
        public UniTask ThrowItem(int itemIndex, Direction8 direction, IWorld world);
    }
}