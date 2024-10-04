using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Utilities;

namespace Domain.Model.Action
{
    public interface IActor : IActorOfEffect
    {
        public Direction8 CurrentDirection { get; }
        public IInventory Inventory { get; }
        public IItemSelector ItemSelector { get; }
        public void DoNothing();
        public bool CanSwap(Direction8 direction, IMap world);
        public UniTask Move(Direction8 direction, IInput input);
        public void Turn(Direction8 direction);
        public UniTask UseSkill(ICharacterSkill skill, Direction8 direction, IMap world);
        public UniTask UseItem(IItem item, Direction8 direction, IMap world);
        public UniTask ThrowItem(IItem item, Direction8 direction, IMap world);
        public float EvaluateThrow(IItem item, Direction8 direction, IMap world);
    }
}