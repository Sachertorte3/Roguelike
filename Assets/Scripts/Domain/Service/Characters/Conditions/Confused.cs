using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Confused : IConditionData
    {
        public string Name => "混乱";
        public ParticleType ParticleType => ParticleType.Confusion;
        public Impact Impact => Impact.Harmful;
        public bool CanAct => true;
        public bool CausesConfusion => true;

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
}