using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Acceleration : IConditionData
    {
        public string Name => "加速";
        public ParticleType ParticleType => ParticleType.FastSpeed;
        public Impact Impact => Impact.Beneficial;
        public bool CanAct => true;
        public bool CausesConfusion => false;

        public void Inflict(IHasCondition hasCondition)
        {
            hasCondition.MultiplyStat(StatType.WaitTime, 0.5f);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
            hasCondition.DivideStat(StatType.WaitTime, 0.5f);
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