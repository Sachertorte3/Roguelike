using Cysharp.Threading.Tasks;
using Domain.Service.Action;

namespace Domain.Service.Characters.Behavior
{
    public interface ICharacterBehavior
    {
        public UniTask<IAction> GenerateNextAction(IHasBehavior character, IMap world, IInput input);
    }
}