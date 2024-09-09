using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using UnityEngine;
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

        public void Inflict(IHasCondition hasCondition)
        {
            hasCondition.RemoveStatMultiplier(StatType.ViewRange, 0.25f);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            Debug.Log("sleep");
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
            hasCondition.AddStatMultiplier(StatType.ViewRange, 0.25f);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.CanAct ? 0 : 0.3f;
        }

        public float EvaluateDamage()
        {
            return 5;
        }
    }
}