using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Utilities;

namespace Model.Domain.Characters.Conditions
{
    internal class Poisoned : IConditionData
    {
        public string Name => "毒";
        public ParticleType ParticleType => ParticleType.PoisoningBubble;
        public Impact Impact => Impact.Harmful;
        public bool CanAct => true;
        public bool CausesConfusion => false;

        public void Inflict(IHasCondition hasCondition)
        {
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            hasCondition.LoseHp(1);
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
        }
    }
}