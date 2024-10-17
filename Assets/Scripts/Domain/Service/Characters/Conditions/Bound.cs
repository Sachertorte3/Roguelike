using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Bound : IConditionData
    {
        public string Name => "拘束";
        public ParticleType ParticleType => ParticleType.None;
        public Impact Impact => Impact.Harmful;
        public string InflictLog => "は拘束された";
        public string DeleteLog => "は移動できるようになった";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddFlagStat(FlagStatType.CannotMove);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveFlagStat(FlagStatType.CannotMove);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.CannotMove ? 0 : CommonSenseParameters.OneTurnStunEquivalentHpReduction / 2;
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.OneTurnStunEquivalentDamage / 2;
        }
    }
}