using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Item;
using Domain.Model.Map;
using Utilities;

namespace Domain.Service.Action
{
    internal record UseItem(IItem Item, Direction8 Direction) : IAction
    {
        public bool Doable(IActor actor, IMap map)
        {
            return !actor.StatusManager.CannotAct &&
            (!Item.IsInfoIdentified(map.Player) || Item.CanActivateWhenUsed);
        }

        public async UniTask Do(IActor actor, IMap map, IInput input)
        {
            await actor.UseItem(Item, Direction, map);
        }

        public float Evaluate(IActor actor, IMap map)
        {
            return Item.EvaluateWhenUsed(actor, actor.Entity.CurrentPosition, Direction, map);
        }

        public string Info()
        {
            return $"UseItem: Item:{Item.DebugInfo()}, Direction:{Direction}";
        }
    }
}