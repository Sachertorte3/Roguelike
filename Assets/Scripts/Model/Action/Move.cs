using Cysharp.Threading.Tasks;
using Scripts.Utilities;

namespace Scripts.Model.Action
{
    internal record Move(Direction8 Direction) : IAction
    {
        private float score;
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
            score = 0;
            return score;
        }
    }
}
