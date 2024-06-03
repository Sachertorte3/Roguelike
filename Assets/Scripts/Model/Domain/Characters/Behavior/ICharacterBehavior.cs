using Cysharp.Threading.Tasks;
using Model.Domain.Action;

namespace Model.Domain.Characters.Behavior
{
    public interface ICharacterBehavior
    {
        public UniTask<IAction> GenerateNextAction(IHasBehavior character, IMap world, IInput input);
    }
}