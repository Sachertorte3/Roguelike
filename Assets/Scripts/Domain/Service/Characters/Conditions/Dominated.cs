using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Dominated : IConditionData
    {
        public string Name => "支配";
        public ParticleType ParticleType => ParticleType.None;
        public Impact Impact => Impact.Harmful;
        public string InflictLog => "は支配された";
        public string DeleteLog => "は支配から解放された";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Affiliation.AddForceAffiliation(actor, AffiliationType.Ally);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Affiliation.RemoveForceAffiliation(actor, AffiliationType.Ally);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return CommonSenseParameters.DamagePerAttack / CommonSenseParameters.MonsterMaxHealth;
        }

        public float EvaluatePrice()
        {
            return 20f;
        }
    }
}