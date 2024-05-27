using Cysharp.Threading.Tasks;
using Utilities;

namespace Model.Domain.Action
{
    internal record UseItem(int ItemIndex, Direction8 Direction) : IAction
    {
        private float score;

        public bool Doable(IActor actor, IWorld world)
        {
            return actor.Inventory.GetItem(ItemIndex).EffectsOnUse;
        }

        public async UniTask Do(IActor actor, IWorld world, IInput input)
        {
            await actor.UseItem(ItemIndex, Direction, world);
        }

        public float Evaluate(IActor actor, IWorld world)
        {
            score = actor.Inventory.GetItem(ItemIndex).Evaluate(actor, actor.CurrentPosition, Direction, world);
            return score;
        }
    }
}