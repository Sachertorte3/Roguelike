using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class SecureHold : IConditionData
    {
        public string Name => "アイテム弾き無効";
        public ParticleType ParticleType => ParticleType.None;
        public Impact Impact => Impact.Beneficial;
        public string InflictLog => "はアイテムを落とさなくなった";
        public string DeleteLog => "";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddFlagStat(FlagStatType.SecureHold);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveFlagStat(FlagStatType.SecureHold);
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