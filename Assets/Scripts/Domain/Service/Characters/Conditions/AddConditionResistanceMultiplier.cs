using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class AddConditionResistanceMultiplier : IConditionData
    {
        public string Name => $"{Condition.name}被付与確率(-{ResistanceRate:P0})";
        public ParticleType ParticleType => ParticleType.BloodRage;
        public Impact Impact => Impact.Beneficial;
        public ConditionTemplate Condition;
        [MinValue(0)] public float ResistanceRate = 0f;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.AddConditionResistance(Condition, ResistanceRate);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.RemoveConditionResistance(Condition, ResistanceRate);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return Condition.Evaluate(target) * ResistanceRate;
        }

        public float EvaluatePrice()
        {
            return Condition.EvaluateDamage() * ResistanceRate;
        }
    }
}