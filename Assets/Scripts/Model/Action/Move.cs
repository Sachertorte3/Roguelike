using Cysharp.Threading.Tasks;
using Scripts.Utilities;

namespace Scripts.Model.Action
{
    internal record Move(Direction8 Direction, float Score = 0) : IAction
    {
        public bool Doable(IActor actor)
        {
            return actor.CanMove(Direction);
        }
        public async UniTask Do(IActor actor)
        {
            await actor.Move(Direction);
        }
        public float Evaluate(IActor actor)
        {
            return Score;
        }
    }
}
