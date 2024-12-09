using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Service.Action
{
    internal record DropItem(ItemFocus Index) : IAction
    {
        public bool Doable(IActor actor, IMap map)
        {
            return !actor.Status.IsFlagStat(FlagStatType.CannotAct);
        }

        public UniTask Do(IActor actor, IMap map, IInput input)
        {
            actor.DropItem(Index, map);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActor actor, IMap map)
        {
            return 0;
        }

        public string Info()
        {
            return $"DropItem: Index:{Index}";
        }
    }
}