using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Slept : IConditionData
    {
        public string Name => "睡眠";
        public ParticleType ParticleType => ParticleType.Sleep;
        public Impact Impact => Impact.Harmful;
        public bool CanAct => false;
        public bool CausesConfusion => false;

        public void Inflict(IHasCondition hasCondition)
        {
            hasCondition.RemoveStatMultiplier(StatType.ViewRange, 0.25f);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
            hasCondition.AddStatMultiplier(StatType.ViewRange, 0.25f);
        }
    }
}