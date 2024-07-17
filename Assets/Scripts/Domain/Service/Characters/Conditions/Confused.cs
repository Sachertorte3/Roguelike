using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Confused : IConditionData
    {
        public string Name => "混乱";
        public ParticleType ParticleType => ParticleType.Confusion;
        public Impact Impact => Impact.Harmful;
        public bool CanAct => true;
        public bool CausesConfusion => true;

        public void Inflict(IHasCondition hasCondition)
        {
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
        }
    }
    internal class NaturalHeal : IConditionData
    {
        public string Name => $"自然治癒({Power})";
        public ParticleType ParticleType => ParticleType.HealGreen;
        public Impact Impact => Impact.Beneficial;
        public bool CanAct => true;
        public bool CausesConfusion => false;
        [MinValue(1)] public int Power = 1;

        public void Inflict(IHasCondition hasCondition)
        {
            hasCondition.AddHpNaturalRecoveryValue(Power);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
            hasCondition.RemoveHpNaturalRecoveryValue(Power);
        }
    }
}