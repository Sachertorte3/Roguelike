using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Domain.Model.Character.Status;
using Domain.Model.Effect;
using Domain.Model.Map;
using Utilities;

namespace Domain.Service.Action
{
    internal record UseSkill(ICharacterSkill Skill, Direction8 Direction) : IAction
    {
        public bool Doable(IActor actor, IMap map)
        {
            return !actor.Status.IsFlagStat(FlagStatType.CannotAct) && Skill.IsUsable();
        }

        public async UniTask Do(IActor actor, IMap map, IInput input)
        {
            await actor.UseSkill(Skill, Direction, map);
        }

        public float Evaluate(IActor actor, IMap map)
        {
            return Skill.Evaluate(actor, actor.Entity.CurrentPosition, Direction, map);
        }

        public string Info()
        {
            return $"UseSkill: \nSkill{Skill.Info()}\nDirection:{Direction}";
        }
    }
}