using Cysharp.Threading.Tasks;
using Data.Condition;
using Data.Effect;
using Utilities;

namespace Model.Domain.Characters.Conditions
{
    internal class Paralyzed : IConditionData
    {
        public string Name => "麻痺";
        public ParticleType ParticleType => ParticleType.Paralysis;
        public Impact Impact => Impact.Harmful;
        public bool CanAct => false;

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