using Cysharp.Threading.Tasks;
using Model.Domain;
using Utilities;

namespace Model.Action
{
    internal record ThrowItem(int ItemIndex, Direction8 Direction) : IAction
    {
        private float score;

        public bool Doable(IActor actor, IWorld world)
        {
            return true;
        }

        public async UniTask Do(IActor actor, IWorld world)
        {
            await actor.ThrowItem(ItemIndex, Direction, world);
        }

        public float Evaluate(IActor actor, IWorld world)
        {
            score = 0;
            return score;
        }
    }
}