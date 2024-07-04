using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Utilities;

namespace Domain.Service.Action
{
    internal record Move(Direction8 Direction, float Score = 0) : IAction
    {
        public bool Doable(IActor actor, IMap world)
        {
            return actor.CanMove(Direction, world);
        }

        public UniTask Do(IActor actor, IMap world, IInput input)
        {
            var _ = actor.Move(Direction, input);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActor actor, IMap world)
        {
            return Score;
        }

        public string Info()
        {
            return $"Move: Direction:{Direction}";
        }
    }
}