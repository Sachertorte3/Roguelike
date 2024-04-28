using Cysharp.Threading.Tasks;
using Scripts.Utilities;

namespace Scripts.Model.Action
{
    internal record Move(Direction8 Direction): IAction
    {
        public UniTask Do(IActor actor)
        {
            actor.Move(Direction);
            return UniTask.CompletedTask;
        }
    }
}
