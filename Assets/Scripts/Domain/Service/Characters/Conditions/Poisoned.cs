using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    [Serializable]
    internal class Poisoned : IConditionData
    {
        public string Name => $"毒(ダメージ:{Power})";
        public ParticleType ParticleType => ParticleType.PoisoningBubble;
        public Impact Impact => Impact.Harmful;
        public bool CanAct => true;
        public bool CausesConfusion => false;
        [MinValue(1)] public int Power = 1;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddStatValue(StatType.HpNaturalRecovery, -Power);
        }

        public UniTask Persist(IHasCondition hasCondition) => UniTask.CompletedTask;

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddStatValue(StatType.HpNaturalRecovery, Power);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return (float)Power / CommonSenseParameters.PlayerMaxHealth;
        }

        public float EvaluatePrice()
        {
            return Power;
        }
    }
}