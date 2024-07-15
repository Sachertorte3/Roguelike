#nullable enable
using Domain.Model.Item;
using UnityEngine;

namespace Domain.Model.Character
{
    public record ItemMemento(
        int Id,
        string Name,
        Sprite Icon,
        ItemState State,
        int Price,
        int MaxUsages,
        int RemainingUsages,
        SkillMemento? SkillOnUse,
        SkillMemento? SkillOnThrow
    );
}