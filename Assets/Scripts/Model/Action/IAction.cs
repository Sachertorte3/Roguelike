using Cysharp.Threading.Tasks;

namespace Scripts.Model.Action
{
    internal interface IAction
    {
        public bool Doable(IActor actor);
        public UniTask Do(IActor actor);
        public float Evaluate(IActor actor);
    }
}