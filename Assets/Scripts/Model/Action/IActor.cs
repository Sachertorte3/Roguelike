using Cysharp.Threading.Tasks;
using Data;
using Model.Characters.Effect;
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
        public bool CanMove(Direction8 direction);
        public UniTask Move(Direction8 direction);
        public void Turn(Direction8 direction);
        public UniTask UseSkill(Skill skill, Direction8 direction);
        public UniTask UseItem(int itemIndex, Direction8 direction);
        public UniTask ThrowItem(int itemIndex, Direction8 direction);
    }
}