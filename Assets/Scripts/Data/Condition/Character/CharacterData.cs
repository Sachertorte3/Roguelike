#nullable enable
using System.Collections.Generic;
using Data.Area;
using Data.Character.Type;
using Data.Condition;
using Data.Effect;
using Effect;
using UnityEngine;

namespace Data.Character
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
        bool EffectsOnUse,
        bool EffectsOnThrow,
        int RemainingUses,
        SkillMemento SkillOnUse,
        SkillMemento SkillOnThrow,
        string Info
    );

    public record SkillMemento(
        IEffectPosition Position,
        IArea Area,
        IEffect Effect
    );

    public record AffiliationMemento(
        int Id,
        CharacterGroup Group,
        Dictionary<int, float> Affiliations
    );
}