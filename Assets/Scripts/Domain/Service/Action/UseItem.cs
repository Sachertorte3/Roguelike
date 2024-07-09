using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Utilities;

namespace Domain.Service.Action
{
    internal record UseItem(int ItemIndex, Direction8 Direction) : IAction
    {
        public bool Doable(IActor actor, IMap world)
        {
            return actor.Inventory.GetItem(ItemIndex).EffectsOnUse;
        }

        public async UniTask Do(IActor actor, IMap world, IInput input)
        {
            await actor.UseItem(ItemIndex, Direction, world);
        }

        public float Evaluate(IActor actor, IMap world)
        {
            return actor.Inventory.GetItem(ItemIndex).Evaluate(actor, actor.CurrentPosition, Direction, world);
        }

        public string Info()
        {
            return $"UseItem: ItemIndex:{ItemIndex}, Direction:{Direction}";
        }
    }
}