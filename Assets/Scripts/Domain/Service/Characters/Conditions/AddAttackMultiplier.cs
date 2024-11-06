using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class AddAttackMultiplier : IConditionData
    {
        public string Name => $"{Element}攻撃倍率(+{AddedMultiplier:P0})";
        public ParticleType ParticleType => ParticleType.BloodRage;
        public Impact Impact => Impact.Beneficial;
        public string InflictLog => $"は{Element}属性の攻撃力が上がった";
        public string DeleteLog => $"の{Element}属性の攻撃力は元に戻った";
        public Element Element;
        [MinValue(0)] public float AddedMultiplier = 0f;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddElementAttackMultiplier(Element, AddedMultiplier);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveElementAttackMultiplier(Element, AddedMultiplier);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return CommonSenseParameters.AttacksPerTurn * CommonSenseParameters.HpReductionPerTurn * AddedMultiplier;
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.AttacksPerTurn * CommonSenseParameters.DamagePerAttack * AddedMultiplier;
        }
    }
    internal class AddResistanceMultiplier : IConditionData
    {
        public string Name => $"{Element}被ダメージ倍率(-{AddedMultiplier:P0})";
        public ParticleType ParticleType => ParticleType.BloodRage;
        public Impact Impact => Impact.Beneficial;
        public string InflictLog => $"は{Element}属性の耐性が上がった";
        public string DeleteLog => $"の{Element}属性の耐性は元に戻った";
        public Element Element;
        [MinValue(0)] public float AddedMultiplier = 0f;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveElementDamageRateMultiplier(Element, AddedMultiplier);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveElementDamageRateMultiplier(Element, AddedMultiplier);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return CommonSenseParameters.AttacksPerTurn * CommonSenseParameters.HpReductionPerTurn * AddedMultiplier;
        }

        public float EvaluatePrice()
        {
            return CommonSenseParameters.AttacksPerTurn * CommonSenseParameters.DamagePerAttack * AddedMultiplier;
        }
    }
}