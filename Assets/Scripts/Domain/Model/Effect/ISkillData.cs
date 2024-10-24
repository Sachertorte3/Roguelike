using System.Collections.Generic;
using Domain.Model.Effect.Area;

namespace Domain.Model.Effect
{
    public interface ISkillData : IHasInfo
    {
        public IEffectPosition Position { get; }
        public IArea Area { get; }
        public List<IEffect> Effects { get; }
        public int Repeats { get; }
        public int RushDistance { get; }
        public int BackStepDistance { get; }
        public float ProbabilityOfSuccess { get; }
        public string Log { get; }
    }
}