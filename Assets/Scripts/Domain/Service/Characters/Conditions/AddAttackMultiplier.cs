using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class AddAttackMultiplier : IConditionData
    {
        public string Name => $"{Element}攻撃倍率(+{AddedMultiplier:P0})";
        public ParticleType ParticleType => ParticleType.BloodRage;
        public Impact Impact => Impact.Beneficial;
        public bool CanAct => true;
        public bool CausesConfusion => false;
        public Element Element;
        [MinValue(0)] public float AddedMultiplier = 0f;

        public void Inflict(IHasCondition hasCondition)
        {
            hasCondition.AddElementAttackMultiplier(Element, AddedMultiplier);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
            hasCondition.RemoveElementAttackMultiplier(Element, AddedMultiplier);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return 0.05f * target.GetElementAttackMultiplier(Element);
        }

        public float EvaluateDamage()
        {
            return 20 * AddedMultiplier;
        }
    }
}