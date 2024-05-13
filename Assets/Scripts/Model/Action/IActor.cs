using Assets.Scripts.Model.Items;
using Cysharp.Threading.Tasks;
using Scripts.Model.Characters.Effect;
using Scripts.Model.Items;
using Scripts.Utilities;
using UnityEngine;

namespace Scripts.Model.Action
{
    public interface IActor
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