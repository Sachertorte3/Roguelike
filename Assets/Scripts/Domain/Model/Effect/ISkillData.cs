using Domain.Model.Effect.Area;

namespace Domain.Model.Effect
{
    public interface ISkillData : IHasInfo
    {
        public IArea Area { get; }
        public IEffect Effect { get; }
        public IEffectPosition Position { get; }
        public int RushDistance { get; }
        public int BackStepDistance { get; }
        public float ProbabilityOfSuccess { get; }
        public string Log { get; }
    }
}