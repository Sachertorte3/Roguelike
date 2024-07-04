using Domain.Model.Area;
using Domain.Model.Effect;
using Effect;

namespace Domain.Model.Character
{
    public record SkillMemento(
        IEffectPosition Position,
        IArea Area,
        IEffect Effect,
        string Info
    );
}