using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class SuperArmor : IConditionData
    {
        public string Name => "スーパーアーマー";
        public ParticleType ParticleType => ParticleType.None;
        public Impact Impact => Impact.Beneficial;
        public string InflictLog => "は吹き飛ばなくなった";
        public string DeleteLog => "";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.AddFlagStat(FlagStatType.Heavy);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.RemoveFlagStat(FlagStatType.Heavy);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.Status.IsFlagStat(FlagStatType.Heavy) ? 0 : 0.1f;
        }

        public float EvaluatePrice()
        {
            return 10f;
        }
    }
}