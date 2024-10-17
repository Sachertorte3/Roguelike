using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Utilities;

namespace Domain.Model.Action
{
    public interface IActor : IActorOfEffect, IHasInventory
    {
        public bool CannotAct { get; }
        public bool CannotMove { get; }
        public Direction8 CurrentDirection { get; }
        public void DoNothing();
        public bool CanSwap(Direction8 direction, IMap map);
        public UniTask Move(Direction8 direction, IInput input);
        public void Turn(Direction8 direction);
        public UniTask UseSkill(ICharacterSkill skill, Direction8 direction, IMap map);
        public UniTask UseItem(IItem item, Direction8 direction, IMap map);
        public UniTask ThrowItem(IItem item, Direction8 direction, IMap map);
        public void DropItem(int itemIndex, IMap map, bool isForced = false);
        public float EvaluateThrow(IItem item, Direction8 direction, IMap map);
    }
}