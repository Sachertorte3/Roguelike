using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Clairvoyant : IConditionData
    {
        public string Name => "千里眼";
        public ParticleType ParticleType => ParticleType.Relieve;
        public Impact Impact => Impact.Beneficial;
        public string InflictLog => "はよく見えるようになった";
        public string DeleteLog => "の視界は元に戻った";
        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddFlagStat(FlagStatType.Clairvoyant);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveFlagStat(FlagStatType.Clairvoyant);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.VisionRange.IsClairvoyant ? 0 : 0.05f;
        }

        public float EvaluatePrice()
        {
            return 10f;
        }
    }
}