using Cysharp.Threading.Tasks;
using Data.Condition;
using Data.Effect;
using Unity.Logging;
using Utilities;

namespace Model.Domain.Characters.Conditions
{
    internal class Star : IConditionData
    {
        public string Name => "☆";
        public ParticleType ParticleType => ParticleType.ShineyStar;
        public Impact Impact => Impact.Beneficial;
        public bool CanAct => true;
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
}