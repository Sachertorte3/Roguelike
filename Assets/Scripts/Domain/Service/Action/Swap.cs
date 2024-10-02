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
            var target = world.GetCharactersInArea(new[] { actor.CurrentPosition + Direction.Vector() })
                .FirstOrDefault();
            if (target == null)
                return false;
            if (target.IsEnemy(actor))
                return false;
            if (target == world.Player)
                return false;
            return target.CanMoveIgnoreCharacter(Direction.Reverse(), world) &&
                   actor.CanMoveIgnoreCharacter(Direction, world);
        }

        public UniTask Do(IActor actor, IMap world, IInput input)
        {
            var target = world.GetCharactersInArea(new[] { actor.CurrentPosition + Direction.Vector() })
                .FirstOrDefault();
            if (target == null)
                throw new InvalidOperationException("target is null");
            var _1 = actor.Move(Direction, input);
            var _2 = target.ForceMove(Direction.Reverse(), input);
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