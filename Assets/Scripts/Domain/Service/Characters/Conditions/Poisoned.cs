using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    [Serializable]
    internal class Poisoned : IConditionData
    {
        public string Name => $"毒({Power})";
        public ParticleType ParticleType => ParticleType.PoisoningBubble;
        public Impact Impact => Impact.Harmful;
        public string InflictLog => "は毒にかかった";
        public string DeleteLog => "は毒が治った";
        [MinValue(0)] public float Power = 1;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.AddStatValue(StatType.HpNaturalRecovery, -Power);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.AddStatValue(StatType.HpNaturalRecovery, Power);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return Power / CommonSenseParameters.PlayerMaxHealth;
        }

        public float EvaluatePrice()
        {
            return Power;
        }
    }
}