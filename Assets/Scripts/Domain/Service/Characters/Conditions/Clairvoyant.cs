using Cysharp.Threading.Tasks;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class Clairvoyant : IConditionData
    {
        public string Name => "千里眼";
        public ParticleType ParticleType => ParticleType.Relieve;
        public Impact Impact => Impact.Beneficial;
        public bool CanAct => true;
        public bool CausesConfusion => false;

        public void Inflict(IHasCondition hasCondition)
        {
            hasCondition.AddClairvoyantFlags();
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
            hasCondition.RemoveClairvoyantFlags();
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.IsClairvoyant ? 0 : 0.05f;
        }

        public float EvaluatePrice()
        {
            return 10f;
        }
    }
    internal class OverDrive : IConditionData
    {
        public string Name => "オーバードライブ";
        public ParticleType ParticleType => ParticleType.None;
        public Impact Impact => Impact.Beneficial;
        public bool CanAct => true;
        public bool CausesConfusion => false;

        public void Inflict(IHasCondition hasCondition)
        {
            hasCondition.AddOverDriveFlags();
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition)
        {
            hasCondition.RemoveOverDriveFlags();
        }

        public float Evaluate(ITargetOfEffect target)
        {
            return target.IsOverDrive ? 0 : 1f;
        }

        public float EvaluatePrice()
        {
            return 20;
        }
    }
}