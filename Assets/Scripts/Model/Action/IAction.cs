using Cysharp.Threading.Tasks;

namespace Scripts.Model.Action
{
    internal interface IAction
    {
        public UniTask Do(IActor actor);
    }
}