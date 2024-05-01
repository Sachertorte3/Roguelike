using Cysharp.Threading.Tasks;
using Scripts.Model.Action;
using Scripts.Utilities;

namespace Scripts.Model.Characters.Behavior
{
    internal sealed class EnemyBehavior : ICharacterBehavior
    {
        public UniTask<IAction> GenerateNextAction()
        {
            return UniTask.FromResult<IAction>(new Move(Direction8.Right));
        }
    }
}
