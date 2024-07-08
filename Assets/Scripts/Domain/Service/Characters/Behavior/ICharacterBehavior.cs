using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Domain.Model.Characters;

namespace Domain.Service.Characters.Behavior
{
    public interface ICharacterBehavior
    {
        public bool WanderAround { get; }
        public UniTask<IAction> GenerateNextAction(IHasBehavior character, IMap world, IInput input);
    }
}