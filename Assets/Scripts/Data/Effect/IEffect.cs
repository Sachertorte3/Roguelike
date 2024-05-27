using Cysharp.Threading.Tasks;

namespace Data.Effect
{
    public interface IEffect : IHasInfo
    {
        public Impact Impact { get; }
        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target);
        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target);
    }
}