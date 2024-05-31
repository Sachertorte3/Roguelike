using Cysharp.Threading.Tasks;
using Utilities;

namespace Model.Domain.Action
{
    internal record ThrowItem(int ItemIndex, Direction8 Direction) : IAction
    {
        private float score;

        public bool Doable(IActor actor, IMap world)
        {
            return true;
        }

        public async UniTask Do(IActor actor, IMap world, IInput input)
        {
            await actor.ThrowItem(ItemIndex, Direction, world);
        }

        public float Evaluate(IActor actor, IMap world)
        {
            score = 0;
            return score;
        }
    }
}