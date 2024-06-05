using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Model.Domain.Characters;
using Utilities;

namespace Model.Domain.Action
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
    }
    internal record Swap(Direction8 Direction, float Score = 0) : IAction
    {
        public bool Doable(IActor actor, IMap world)
        {
            var target = world.GetCharactersInArea(new[] { actor.CurrentPosition + Direction.Vector() }).FirstOrDefault();
            if (target == null)
                return false;
            if (target.IsEnemy(actor))
                return false;
            return target.CanMoveIgnoreCharacter(Direction.Reverse(), world) && actor.CanMoveIgnoreCharacter(Direction, world);
        }

        public UniTask Do(IActor actor, IMap world, IInput input)
        {
            var target = world.GetCharactersInArea(new[] { actor.CurrentPosition + Direction.Vector() }).FirstOrDefault();
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
    }
}