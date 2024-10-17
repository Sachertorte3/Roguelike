using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Paralyzed : IConditionData
    {
        public string Name => "麻痺";
        public ParticleType ParticleType => ParticleType.Paralysis;
        public Impact Impact => Impact.Harmful;
        public string InflictLog => "は麻痺した";
        public string DeleteLog => "は体が動くようになった";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddFlagStat(FlagStatType.CannotAct);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveFlagStat(FlagStatType.CannotAct);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.CannotAct ? 0 : CommonSenseParameters.OneTurnStunEquivalentHpReduction;
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.OneTurnStunEquivalentDamage;
        }
    }
}