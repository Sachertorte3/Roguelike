using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character;

namespace Domain.Service.Characters.Behavior
{
    public interface ICharacterBehavior
    {
        public bool WanderAround { get; }
        public UniTask<IAction> GenerateNextAction(IHasBehavior character, IMap world, IInput input);
    }
}