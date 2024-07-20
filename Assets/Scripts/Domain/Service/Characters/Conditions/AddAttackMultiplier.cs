using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class AddAttackMultiplier : IConditionData
    {
        public string Name => $"攻撃倍率(+{AddedMultiplier:P0})";
        public ParticleType ParticleType => ParticleType.BloodRage;
        public Impact Impact => Impact.Beneficial;
        public bool CanAct => true;
        public bool CausesConfusion => false;
        [MinValue(0)] public float AddedMultiplier = 0f;

        public void Inflict(IHasCondition hasCondition)
        {
            hasCondition.AddStatMultiplier(StatType.AttackMultiplier, AddedMultiplier);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
            hasCondition.RemoveStatMultiplier(StatType.AttackMultiplier, AddedMultiplier);
        }
    }
}