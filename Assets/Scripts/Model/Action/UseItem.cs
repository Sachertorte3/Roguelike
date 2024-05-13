using Cysharp.Threading.Tasks;
using Scripts.Model.Characters.Effect;
using Scripts.Utilities;

namespace Scripts.Model.Action
{
    internal record UseItem(int ItemIndex, Direction8 Direction) : IAction
    {
        private float score;
        public bool Doable(IActor actor)
        {
            return true;
        }
        public async UniTask Do(IActor actor)
        {
            await actor.UseItem(ItemIndex, Direction);
        }
        public float Evaluate(IActor actor)
        {
            score = actor.Inventory.GetItem(ItemIndex).Evaluate(actor, Direction);
            return score;
        }
    }
    internal record ThrowItem(int ItemIndex, Direction8 Direction) : IAction
    {
        private float score;
        public bool Doable(IActor actor)
        {
            return true;
        }
        public async UniTask Do(IActor actor)
        {
            await actor.ThrowItem(ItemIndex, Direction);
        }
        public float Evaluate(IActor actor)
        {
            score = actor.Inventory.GetItem(ItemIndex).Evaluate(actor, Direction);
            return score;
        }
    }
    internal record UseSkill(Skill Skill, Direction8 Direction) : IAction
    {
        private float score;
        public bool Doable(IActor actor)
        {
            return true;
        }
        public async UniTask Do(IActor actor)
        {
            await actor.UseSkill(Skill, Direction);
        }
        public float Evaluate(IActor actor)
        {
            score = Skill.Evaluate(actor, Direction);
            return score;
        }
    }
}
