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
        /// <summary>
        /// Calculates the expected profit for the Actor when doing the action.
        /// It doesn't care if it's doable or not.
        /// </summary>
        /// <param name="actor">The actor of this action.</param>
        /// <returns></returns>
        public float Evaluate(IActor actor)
        {
            score = 1;
            return score;
        }
    }
}
