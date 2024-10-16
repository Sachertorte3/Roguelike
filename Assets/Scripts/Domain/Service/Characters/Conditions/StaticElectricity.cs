using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class StaticElectricity : IConditionData
    {
        public string Name => "静電気";
        public ParticleType ParticleType => ParticleType.Paralysis;
        public Impact Impact => Impact.Harmful;
        public bool CanAct => true;
        public bool CausesConfusion => false;
        public string InflictLog => "は静電気を帯びた";
        public string DeleteLog => "は静電気が抜けた";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddFlagStat(FlagStatType.IsAffectedByTrap);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveFlagStat(FlagStatType.IsAffectedByTrap);
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