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
    }
    internal class Clairvoyant : IConditionData
    {
        public string Name => "千里眼";
        public ParticleType ParticleType => ParticleType.Relieve;
        public Impact Impact => Impact.Beneficial;
        public bool CanAct => true;
        public bool CausesConfusion => false;

        public void Inflict(IHasCondition hasCondition)
        {
            hasCondition.AddClairvoyantFlags();
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
            hasCondition.RemoveClairvoyantFlags();
        }
    }
}