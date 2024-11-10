using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Haggle : IConditionData
    {
        public string Name => "値切り";
        public ParticleType ParticleType => ParticleType.None;
        public Impact Impact => Impact.Beneficial;
        public string InflictLog => "は値切り交渉がうまくなった";
        public string DeleteLog => "";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.AddFlagStat(FlagStatType.Haggle);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.RemoveFlagStat(FlagStatType.Haggle);
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