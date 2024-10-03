using Domain.Model.Effect.Area;

namespace Domain.Model.Effect
{
    public interface IActorlessSkillData : ISkillData
    {
        public INotDirectionalArea Area { get; }
        IArea ISkillData.Area => Area;
        public IActorlessEffect Effect { get; }
        IEffect ISkillData.Effect => Effect;
        public IActorlessEffectPosition Position { get; }
        IEffectPosition ISkillData.Position => Position;
    }
}