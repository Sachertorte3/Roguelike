using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Slept : IConditionData
    {
        public string Name => "睡眠";
        public ParticleType ParticleType => ParticleType.Sleep;
        public Impact Impact => Impact.Harmful;
        public string InflictLog => "は眠りについた";
        public string DeleteLog => "は眠りから覚めた";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddFlagStat(FlagStatType.CannotAct);
            hasCondition.StatusManager.AddFlagStat(FlagStatType.Blind);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveFlagStat(FlagStatType.CannotAct);
            hasCondition.StatusManager.RemoveFlagStat(FlagStatType.Blind);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.StatusManager.CannotAct ? 0 : CommonSenseParameters.OneTurnStunEquivalentHpReduction;
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.OneTurnStunEquivalentDamage;
        }
    }
}