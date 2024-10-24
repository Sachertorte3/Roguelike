using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class CurseProof : IConditionData
    {
        public string Name => "呪い無効";
        public ParticleType ParticleType => ParticleType.None;
        public Impact Impact => Impact.Beneficial;
        public string InflictLog => "は呪いを受けなくなった";
        public string DeleteLog => "";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddFlagStat(FlagStatType.CurseProof);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveFlagStat(FlagStatType.CurseProof);
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