#nullable enable
using System.Collections.Generic;
using Domain.Model.Condition;
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
        SkillMemento? SkillOnUse,
        SkillMemento? SkillOnThrow,
        int MaxUsages,
        int RemainingUsages,
        List<IConditionData> Conditions
    );
}