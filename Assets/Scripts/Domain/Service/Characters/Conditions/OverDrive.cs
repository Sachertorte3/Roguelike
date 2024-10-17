using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Utilities;

namespace Domain.Service.Characters.Conditions
{
    internal class OverDrive : IConditionData
    {
        public string Name => "オーバードライブ";
        public ParticleType ParticleType => ParticleType.None;
        public Impact Impact => Impact.Beneficial;
        public string InflictLog => "以外の時が止まった";
        public string DeleteLog => "以外の時が動き出した";

        public void Inflict(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.AddFlagStat(FlagStatType.OverDrive);
        }

        public UniTask Persist(IHasCondition hasCondition)
        {
            return UniTask.CompletedTask;
        }

        public void Delete(IHasCondition hasCondition, Id<IEntity> actor)
        {
            hasCondition.StatusManager.RemoveFlagStat(FlagStatType.OverDrive);
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