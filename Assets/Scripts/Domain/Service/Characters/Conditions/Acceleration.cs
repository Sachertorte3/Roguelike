using Cysharp.Threading.Tasks;
using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Acceleration : IConditionData
    {
        public string Name => "加速";
        public ParticleType ParticleType => ParticleType.FastSpeed;
        public Impact Impact => Impact.Beneficial;

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.MultiplyStat(StatType.MaxWaitTime, 0.5f);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.Status.DivideStat(StatType.MaxWaitTime, 0.5f);
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return 1 / CommonSenseParameters.AttacksToDefeatPlayer;
        }

        public float EvaluatePrice()
        {
            return 20f;
        }
    }
}