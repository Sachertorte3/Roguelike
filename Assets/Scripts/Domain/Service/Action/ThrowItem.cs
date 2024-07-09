using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Action;
using Utilities;

namespace Domain.Service.Action
{
    internal record ThrowItem(int ItemIndex, Direction8 Direction) : IAction
    {
        public bool Doable(IActor actor, IMap world)
        {
            return true;
        }

        public async UniTask Do(IActor actor, IMap world, IInput input)
        {
            await actor.ThrowItem(ItemIndex, Direction, world);
        }

        public float Evaluate(IActor actor, IMap world)
        {
            return 0;
        }

        public string Info()
        {
            return $"ThrowItem: ItemIndex:{ItemIndex}, Direction:{Direction}";
        }
    }
}