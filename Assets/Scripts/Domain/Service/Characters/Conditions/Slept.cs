using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
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
            hasCondition.Status.AddFlagStat(FlagStatType.CannotAct);
            hasCondition.Status.AddFlagStat(FlagStatType.Blind);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.RemoveFlagStat(FlagStatType.CannotAct);
            hasCondition.Status.RemoveFlagStat(FlagStatType.Blind);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.Status.IsFlagStat(FlagStatType.CannotAct) ? 0 : CommonSenseParameters.OneTurnStunEquivalentHpReduction;
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.OneTurnStunEquivalentDamage;
        }
    }
}