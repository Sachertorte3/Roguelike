using Cysharp.Threading.Tasks;
using Domain.Model;
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
        public string InflictLog => "は自然治癒力が上がった";
        public string DeleteLog => "は自然治癒力はもとに戻った";
        [MinValue(0)] public float Power;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddStatValue(StatType.HpNaturalRecovery, Power);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveStatValue(StatType.HpNaturalRecovery, Power);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return Power / target.CurrentMaxHp;
        }

        public float EvaluatePrice()
        {
            return Power;
        }
    }
}