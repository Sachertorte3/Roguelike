using Cysharp.Threading.Tasks;
using Model.Domain;

namespace Model.Action
{
    public interface IAction
    {
        public bool Doable(IActor actor, IWorld world);
        public UniTask Do(IActor actor, IWorld world, IInput input);

        /// <summary>
        ///     Calculates the expected profit for the Actor when doing the action.
        ///     It doesn't care if it's doable or not.
        /// </summary>
        /// <param name="actor">The actor of this action.</param>
        /// <returns></returns>
        public float Evaluate(IActor actor, IWorld world);
    }
}

