using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Map;

namespace Domain.Service.Action
{
    internal record DropItem(int ItemIndex) : IAction
    {
        public bool Doable(IActor actor, IMap map)
        {
            return !actor.Status.IsFlagStat(FlagStatType.CannotAct);
        }

        public UniTask Do(IActor actor, IMap map, IInput input)
        {
            actor.DropItem(ItemIndex, map);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActor actor, IMap map)
        {
            return 0;
        }

        public string Info()
        {
            return $"DropItem: Item:{ItemIndex}";
        }
    }
}