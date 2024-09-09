using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Model.Condition
{
    public interface IConditionData
    {
        public string Name { get; }
        public ParticleType ParticleType { get; }
        public Impact Impact { get; }
        public bool CanAct { get; }
        public bool CausesConfusion { get; }
        public void Inflict(IHasCondition hasCondition);
        public UniTask Persist(IHasCondition hasCondition);
        public void Delete(IHasCondition hasCondition);
        public float Evaluate(ITargetOfEffect target);
        public float EvaluatePrice();
    }
}