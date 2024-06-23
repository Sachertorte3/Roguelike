using Cysharp.Threading.Tasks;
using Utilities;

namespace Model.Domain.Action
{
    internal record UseItem(int ItemIndex, Direction8 Direction) : IAction
    {
        private float score;

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
            score = actor.Inventory.GetItem(ItemIndex).Evaluate(actor, actor.CurrentPosition, Direction, world);
            return score;
        }

        public string Info()
        {
            return $"UseItem: ItemIndex:{ItemIndex}, Direction:{Direction}";
        }
    }
}