using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
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

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.IsConfused ? 0 : CommonSenseParameters.OneTurnStunEquivalentHpReduction / 2;
        }

        public float EvaluatePrice()
        {
            return 5f;
        }
    }
}