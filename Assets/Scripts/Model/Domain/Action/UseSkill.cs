using Cysharp.Threading.Tasks;
using Model.Effect;
using Utilities;

namespace Model.Action
{
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
            score = Skill.Evaluate(actor, actor.CurrentPosition, Direction);
            return score;
        }
    }
}