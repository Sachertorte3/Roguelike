using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Map;
using Utilities;

namespace Domain.Service.Action
{
    internal record Move(Direction8 Direction, float Score = 0) : IAction
    {
        public bool Doable(IActor actor, IMap map)
        {
            return !actor.StatusManager.CannotAct && !actor.StatusManager.CannotMove && actor.CanMove(Direction, map);
        }

        public UniTask Do(IActor actor, IMap map, IInput input)
        {
            actor.Move(Direction, input).Forget();
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActor actor, IMap map)
        {
            return Score;
        }

        public string Info()
        {
            return $"Move: Direction:{Direction}";
        }
    }
}