using Cysharp.Threading.Tasks;

namespace Scripts.Model.Action
{
    internal interface IAction
    {
        public bool Doable(IActor actor);
        public UniTask Do(IActor actor);
        /// <summary>
        /// Calculates the expected profit for the Actor when doing the action.
        /// It doesn't care if it's doable or not.
        /// </summary>
        /// <param name="actor">The actor of this action.</param>
        /// <returns></returns>
        public float Evaluate(IActor actor);
    }
}