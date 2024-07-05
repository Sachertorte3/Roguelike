using UnityEngine;

namespace Domain.Model.Character
{
    public record ItemMemento(
        int Id,
        string Name,
        Sprite Icon,
        int Price,
        int MaxUsages,
        int RemainingUsages,
        SkillMemento? SkillOnUse,
        SkillMemento? SkillOnThrow
    );
}