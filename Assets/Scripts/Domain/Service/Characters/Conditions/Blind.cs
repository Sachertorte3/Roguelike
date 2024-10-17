using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Blind : IConditionData
    {
        public string Name => "盲目";
        public ParticleType ParticleType => ParticleType.Blind;
        public Impact Impact => Impact.Harmful;
        public string InflictLog => "は盲目になった";
        public string DeleteLog => "の盲目は元に戻った";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddFlagStat(FlagStatType.Blind);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveFlagStat(FlagStatType.Blind);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.CannotAct ? 0 : CommonSenseParameters.OneTurnStunEquivalentHpReduction / 2;
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.OneTurnStunEquivalentDamage / 2;
        }
    }
}