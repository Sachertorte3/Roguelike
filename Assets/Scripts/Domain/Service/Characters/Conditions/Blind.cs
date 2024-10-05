using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Blind : IConditionData
    {
        public string Name => "盲目";
        public ParticleType ParticleType => ParticleType.Blind;
        public Impact Impact => Impact.Harmful;
        public bool CanAct => true;
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
            return target.CanAct ? CommonSenseParameters.OneTurnStunEquivalentHpReduction / 2 : 0;
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.OneTurnStunEquivalentDamage / 2;
        }
    }
}