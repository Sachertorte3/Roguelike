using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
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
            Log.Debug("Star inflict");
            hasCondition.AddMaxHpMultiplier(2);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
            Log.Debug("Star delete");
            hasCondition.RemoveMaxHpMultiplier(2);
        }
    }
}