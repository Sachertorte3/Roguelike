using UnityEngine;

namespace Domain.Model.Character
{
    public record ItemMemento(
        string Name,
        Sprite Icon,
        int Price,
        int MaxUsages,
        int RemainingUsages,
        SkillMemento? SkillOnUse,
        SkillMemento? SkillOnThrow
    );
}