using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Item;
using Utilities;

namespace Domain.Service.Action
{
    internal record UseItem(IItem Item, Direction8 Direction) : IAction
    {
        public bool Doable(IActor actor, IMap world)
        {
            return Item.CanActivateWhenUsed;
        }

        public async UniTask Do(IActor actor, IMap world, IInput input)
        {
            await actor.UseItem(Item, Direction, world);
        }

        public float Evaluate(IActor actor, IMap world)
        {
            return Item.EvaluateWhenUsed(actor, actor.CurrentPosition, Direction, world);
        }

        public string Info()
        {
            return $"UseItem: Item:{Item.Info()}, Direction:{Direction}";
        }
    }
}