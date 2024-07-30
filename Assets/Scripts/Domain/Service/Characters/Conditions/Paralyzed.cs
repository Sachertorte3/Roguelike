using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Paralyzed : IConditionData
    {
        public string Name => "麻痺";
        public ParticleType ParticleType => ParticleType.Paralysis;
        public Impact Impact => Impact.Harmful;
        public bool CanAct => false;
        public bool CausesConfusion => false;

        public void Inflict(IHasCondition hasCondition)
        {
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.CanAct ? 0 : 0.3f;
        }
    }
}