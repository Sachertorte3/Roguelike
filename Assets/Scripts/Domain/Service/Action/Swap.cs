using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Map;
using Utilities;

namespace Domain.Service.Action
{
    internal record Swap(Direction8 Direction, float Score = 0) : IAction
    {
        public bool Doable(IActor actor, IMap world)
        {
            return actor.CanSwap(Direction, world);
        }

        public UniTask Do(IActor actor, IMap world, IInput input)
        {
            var target = world.GetCharactersInArea(new[] { actor.CurrentPosition + Direction.Vector() })
                .FirstOrDefault();
            if (target == null)
                throw new InvalidOperationException("target is null");
            actor.Move(Direction, input).Forget();
            target.ForceMove(Direction.Reverse(), input).Forget();
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActor actor, IMap world)
        {
            return Score;
        }

        public string Info()
        {
            return $"Swap: Direction:{Direction}";
        }
    }
}