using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Protected : IConditionData
    {
        public string Name => "防護";
        public ParticleType ParticleType => ParticleType.PowerUp;
        public Impact Impact => Impact.Beneficial;
        public bool CanAct => true;
        public bool CausesConfusion => false;
        public string InflictLog => "は守りを固めた";
        public string DeleteLog => "の守りは元に戻った";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddFlagStat(FlagStatType.Hard);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveFlagStat(FlagStatType.Hard);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return CommonSenseParameters.DamagePerAttack / CommonSenseParameters.MonsterMaxHealth;
        }

        public float EvaluatePrice()
        {
            return 20f;
        }
    }
}