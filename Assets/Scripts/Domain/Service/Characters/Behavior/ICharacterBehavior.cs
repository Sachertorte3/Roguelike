using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;
using Domain.Model.Item;
using R3;

namespace Domain.Service.Characters.Behavior
{
    public interface  ICharacterBehavior : IItemSelecter
    {
        public BehaviorData BehaviorData { get; }
        public ReadOnlyReactiveProperty<bool> IsWaitingItemSelect { get; }
        public UniTask<IAction> GenerateNextAction(IHasBehavior character, IMap world, IInput input);
    }
}