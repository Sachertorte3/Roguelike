using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Slow : IConditionData
    {
        public string Name => "鈍足";
        public ParticleType ParticleType => ParticleType.SlowDown;
        public Impact Impact => Impact.Harmful;
        public bool CanAct => true;
        public bool CausesConfusion => false;

        public void Inflict(IHasCondition hasCondition)
        {
            hasCondition.DivideStat(StatType.WaitTime, 0.5f);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
            hasCondition.MultiplyStat(StatType.WaitTime, 0.5f);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return 0.1f;
        }

        public float EvaluateDamage()
        {
            return 20;
        }
    }
}