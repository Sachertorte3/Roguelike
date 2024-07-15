#nullable enable
using Domain.Model.Effect.Area;
using Domain.Model.Effect;

namespace Domain.Model.Character
{
    public record SkillMemento(
        IEffectPosition Position,
        IArea Area,
        IEffect Effect,
        string Info,
        string? Log
    );
}