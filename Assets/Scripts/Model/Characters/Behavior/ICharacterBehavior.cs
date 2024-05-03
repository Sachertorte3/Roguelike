using Cysharp.Threading.Tasks;
using Scripts.Model.Action;

namespace Scripts.Model.Characters.Behavior
{
    internal interface ICharacterBehavior
    {
        public UniTask<IAction> GenerateNextAction(IHasBehavior character);
    }
}