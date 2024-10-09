using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class SuperArmor : IConditionData
    {
        public string Name => "スーパーアーマー";
        public ParticleType ParticleType => ParticleType.None;
        public Impact Impact => Impact.Beneficial;
        public bool CanAct => true;
        public bool CausesConfusion => false;
        public string InflictLog => "は吹き飛ばなくなった";
        public string DeleteLog => "";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddFlagStat(FlagStatType.Heavy);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveFlagStat(FlagStatType.Heavy);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return 0.1f;
        }

        public float EvaluatePrice()
        {
            return 10f;
        }
    }
}