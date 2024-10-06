using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Slept : IConditionData
    {
        public string Name => "睡眠";
        public ParticleType ParticleType => ParticleType.Sleep;
        public Impact Impact => Impact.Harmful;
        public bool CanAct => false;
        public bool CausesConfusion => false;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddBlindFlags();
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveBlindFlags();
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.CanAct ? CommonSenseParameters.OneTurnStunEquivalentHpReduction : 0;
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.OneTurnStunEquivalentDamage;
        }
    }
}