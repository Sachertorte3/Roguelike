using Cysharp.Threading.Tasks;
using Model.Domain;
using Utilities;

namespace Model.Action
{
    internal record Move(Direction8 Direction, float Score = 0) : IAction
    {
        public bool Doable(IActor actor, IWorld world)
        {
            return actor.CanMove(Direction, world);
        }

        public UniTask Do(IActor actor, IWorld world)
        {
            var _ = actor.Move(Direction, world);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActor actor, IWorld world)
        {
            return Score;
        }
    }
}