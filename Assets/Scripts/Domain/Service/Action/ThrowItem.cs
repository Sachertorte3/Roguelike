using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Item;
using Utilities;

namespace Domain.Service.Action
{
    internal record ThrowItem(IItem Item, Direction8 Direction) : IAction
    {
        public bool Doable(IActor actor, IMap world)
        {
            return true;
        }

        public async UniTask Do(IActor actor, IMap world, IInput input)
        {
            await actor.ThrowItem(Item, Direction, world);
        }

        public float Evaluate(IActor actor, IMap world)
        {
            return actor.EvaluateThrow(Item, Direction, world);
        }

        public string Info()
        {
            return $"ThrowItem: Item:{Item.Info()}, Direction:{Direction}";
        }
    }
}