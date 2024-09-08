using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class AddMaxHp : IConditionData
    {
        public string Name => $"最大HP(+{AddValue})";
        public ParticleType ParticleType => ParticleType.None;
        public Impact Impact => Impact.Beneficial;
        public bool CanAct => true;
        public bool CausesConfusion => false;
        [MinValue(0)] public int AddValue;

        public void Inflict(IHasCondition hasCondition)
        {
            hasCondition.AddStatValue(StatType.MaxHp, AddValue);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
            hasCondition.RemoveStatValue(StatType.MaxHp, AddValue);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return 0.05f * target.GetStatValue(StatType.MaxHp);
        }

        public float EvaluateDamage()
        {
            return AddValue;
        }
    }
}