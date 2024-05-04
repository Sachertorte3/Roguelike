using Cysharp.Threading.Tasks;
using Scripts.Utilities;

namespace Scripts.Model.Action
{
    internal record Move(Direction8 Direction, float Score=0) : IAction
    {
        public bool Doable(IActor actor)
        {
            return actor.CanMove(Direction);
        }
        public UniTask Do(IActor actor)
        {
            actor.Move(Direction);
            return UniTask.CompletedTask;
        }
        public float Evaluate(IActor actor)
        {
            return Score;
        }
    }
}
