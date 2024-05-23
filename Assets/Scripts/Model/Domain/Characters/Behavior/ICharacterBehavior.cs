using Cysharp.Threading.Tasks;
using Model.Action;

namespace Model.Characters.Behavior
{
    internal interface ICharacterBehavior
    {
        public UniTask<IAction> GenerateNextAction(IHasBehavior character);
    }
}