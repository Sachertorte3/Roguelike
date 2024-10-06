using Cysharp.Threading.Tasks;
using Domain.Model;
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
        public string InflictLog => $"は最大HPが上がった";
        public string DeleteLog => $"の最大HPは元に戻った";
        [MinValue(0)] public int AddValue;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddStatValue(StatType.MaxHp, AddValue);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveStatValue(StatType.MaxHp, AddValue);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return AddValue / target.GetStatValue(StatType.MaxHp);
        }

        public float EvaluatePrice()
        {
            return AddValue;
        }
    }
}