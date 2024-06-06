using Cysharp.Threading.Tasks;
using Data.Effect;
using Utilities;

namespace Data.Condition
{
    public interface IConditionData
    {
        public string Name { get; }
        public ParticleType ParticleType { get; }
        public Impact Impact { get; }
        public bool CanAct { get; }
        public void Inflict(IHasCondition hasCondition);
        public UniTask Persist(IHasCondition hasCondition);
        public void Delete(IHasCondition hasCondition);
    }
}