using Cysharp.Threading.Tasks;
using Utilities;

namespace Model.Action
{
    internal record UseItem(int ItemIndex, Direction8 Direction) : IAction
    {
        private float score;

        public bool Doable(IActor actor)
        {
            return actor.Inventory.GetItem(ItemIndex).EffectsOnUse;
        }

        public async UniTask Do(IActor actor)
        {
            await actor.UseItem(ItemIndex, Direction);
        }

        public float Evaluate(IActor actor)
        {
            score = actor.Inventory.GetItem(ItemIndex).Evaluate(actor, actor.CurrentPosition, Direction);
            return score;
        }
    }
}