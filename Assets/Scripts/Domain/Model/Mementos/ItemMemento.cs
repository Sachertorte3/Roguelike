using UnityEngine;

namespace Domain.Model.Character
{
    public record ItemMemento(
        string Name,
        Sprite Icon,
        int MaxUsages,
        int RemainingUsages,
        SkillMemento? SkillOnUse,
        SkillMemento? SkillOnThrow
    );
}