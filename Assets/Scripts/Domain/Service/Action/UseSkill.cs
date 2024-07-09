using Cysharp.Threading.Tasks;
using Domain.Model.Action;
using Utilities;

namespace Domain.Service.Action
{
    internal record UseSkill(ISkill Skill, Direction8 Direction) : IAction
    {
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
            return Skill.Evaluate(actor, actor.CurrentPosition, Direction, world);
        }

        public string Info()
        {
            return $"UseSkill: \nSkill{Skill.Info()}\nDirection:{Direction}";
        }
    }
}