#nullable enable
using System.Collections.Generic;
using Domain.Model.Area;
using Domain.Model.Character.Type;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Effect;
using UnityEngine;

namespace Domain.Model.Character
{
    public record CharacterMemento(
        string Name,
        ICharacterType CharacterType,
        CharacterStatusMemento Status,
        EntityMemento EntityData,
        SkillMemento[] Skills,
        InventoryMemento Inventory,
        AffiliationMemento Affiliation,
        Aggression Aggression,
        bool IsLeader
    );

    public record EntityMemento(
        Vector2Int Position,
        EntityLayer Layer
    );

    public record CharacterStatusMemento(
        int MaxHp,
        int Hp,
        ConditionMemento[] Conditions
    );

    public record ConditionMemento(
        IConditionData Condition,
        RemovalConditionData RemovalCondition,
        int ElapsedTurns
    );

    public record InventoryMemento(
        ItemMemento?[] Items
    );

    public record ItemMemento(
        string Name,
        Sprite Icon,
        int MaxUsages,
        int RemainingUsages,
        SkillMemento? SkillOnUse,
        SkillMemento? SkillOnThrow
    );

    public record SkillMemento(
        IEffectPosition Position,
        IArea Area,
        IEffect Effect,
        string Info
    );

    public record AffiliationMemento(
        int Id,
        CharacterGroup Group,
        Dictionary<int, float> Affiliations
    );
}