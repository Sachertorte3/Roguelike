using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
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
            hasCondition.AddStatValue(StatType.HpNaturalRecovery, Power);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
            hasCondition.RemoveStatValue(StatType.HpNaturalRecovery, Power);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return (float)Power / target.CurrentMaxHp;
        }

        public float EvaluatePrice()
        {
            return Power;
        }
    }
}