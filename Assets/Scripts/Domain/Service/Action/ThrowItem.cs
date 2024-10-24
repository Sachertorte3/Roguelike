using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Item;
using Domain.Model.Map;
using Utilities;

namespace Domain.Service.Action
{
    internal record ThrowItem(IItem Item, Direction8 Direction) : IAction
    {
        public bool Doable(IActor actor, IMap map)
        {
            return !actor.StatusManager.CannotAct;
        }

        public async UniTask Do(IActor actor, IMap map, IInput input)
        {
            await actor.ThrowItem(Item, Direction, map);
        }

        public float Evaluate(IActor actor, IMap map)
        {
            return actor.EvaluateThrow(Item, Direction, map);
        }

        public string Info()
        {
            return $"ThrowItem: Item:{Item.DebugInfo()}, Direction:{Direction}";
        }
    }
}