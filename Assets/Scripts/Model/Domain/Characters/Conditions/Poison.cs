using Cysharp.Threading.Tasks;
using Data;
using Data.Condition;
using Utilities;

namespace Model.Characters.Conditions
{
    internal class Poison : IConditionData
    {
        public string Name => "毒";
        public ParticleType ParticleType => ParticleType.PoisoningBubble;
        public Impact Impact => Impact.Harmful;

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
