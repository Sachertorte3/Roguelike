using Cysharp.Threading.Tasks;
using Model.Domain.Effect;
using Utilities;

namespace Model.Domain.Action
{
    internal record UseSkill(Skill Skill, Direction8 Direction) : IAction
    {
        private float score;

        public bool Doable(IActor actor, IMap world)
        {
            return true;
        }

        public async UniTask Do(IActor actor, IMap world, IInput input)
        {
            await actor.UseSkill(Skill, Direction, world);
        }

        public float Evaluate(IActor actor, IMap world)
        {
            score = Skill.Evaluate(actor, actor.CurrentPosition, Direction, world);
            return score;
        }
    }
}