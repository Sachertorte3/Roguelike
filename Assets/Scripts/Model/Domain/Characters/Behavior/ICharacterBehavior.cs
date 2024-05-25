using Cysharp.Threading.Tasks;
using Model.Action;
using Model.Domain;

namespace Model.Characters.Behavior
{
    public interface ICharacterBehavior
    {
        public UniTask<IAction> GenerateNextAction(IHasBehavior character, IWorld world, IInput input);
    }
}