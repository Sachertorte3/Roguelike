using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Slowness : IConditionData
    {
        public string Name => "鈍足";
        public ParticleType ParticleType => ParticleType.SlowDown;
        public Impact Impact => Impact.Harmful;
        public string InflictLog => "は足が遅くなった";
        public string DeleteLog => "は元の速度に戻った";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.DivideStat(StatType.WaitTime, 0.5f);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.MultiplyStat(StatType.WaitTime, 0.5f);
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