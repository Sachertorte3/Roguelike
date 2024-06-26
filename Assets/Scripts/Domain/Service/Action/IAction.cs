using Cysharp.Threading.Tasks;
using Domain.Model;

namespace Model.Domain.Action
{
    public interface IAction : IHasInfo
    {
        public bool Doable(IActor actor, IMap world);
        public UniTask Do(IActor actor, IMap world, IInput input);

        /// <summary>
        ///     Calculates the expected profit for the Actor when doing the action.
        ///     It doesn't care if it's doable or not.
        /// </summary>
        /// <param name="actor">The actor of this action.</param>
        /// <returns></returns>
        public float Evaluate(IActor actor, IMap world);
    }
}