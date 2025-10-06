using Cysharp.Threading.Tasks;
using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class FlagCondition : IConditionData
    {
        [Required, SerializeField] private FlagStatType _flagStatType;
        public string Name => _flagStatType.GetName();
        public ParticleType ParticleType => _flagStatType.GetParticleType();
        public Impact Impact => _flagStatType.GetImpact();

        public FlagCondition(FlagStatType flagStatType)
        {
            _flagStatType = flagStatType;
        }

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetFlagStat(_flagStatType).Add();
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.GetFlagStat(_flagStatType).Remove();
        }

        public float Evaluate(ITargetOfEffect target)
        {
            if (target.Status.GetFlagStat(_flagStatType).CurrentValue)
            {
                return _flagStatType.Evaluate(target);
            }
            return 0;
        }
        public float EvaluatePrice() => _flagStatType.EvaluatePrice();
    }
}