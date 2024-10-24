using System.Collections.Generic;
using System.Linq;
using Domain.Model.Effect.Area;

namespace Domain.Model.Effect
{
    public interface IActorlessSkillData : ISkillData
    {
        public INotDirectionalArea Area { get; }
        IArea ISkillData.Area => Area;
        public List<IActorlessEffect> Effects { get; }
        List<IEffect> ISkillData.Effects => Effects.Cast<IEffect>().ToList();
        public IActorlessEffectPosition Position { get; }
        IEffectPosition ISkillData.Position => Position;
    }
}