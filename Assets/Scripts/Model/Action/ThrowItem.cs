using Cysharp.Threading.Tasks;
using Utilities;

namespace Model.Action
{
    internal record ThrowItem(int ItemIndex, Direction8 Direction) : IAction
    {
        private float score;

        public bool Doable(IActor actor)
        {
            return true;
        }

        public async UniTask Do(IActor actor)
        {
            await actor.ThrowItem(ItemIndex, Direction);
        }

        public float Evaluate(IActor actor)
        {
            score = 0;
            return score;
        }
    }
}