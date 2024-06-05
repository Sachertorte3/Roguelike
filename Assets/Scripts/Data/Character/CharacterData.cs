#nullable enable
using System.Collections.Generic;
using Data;
using Data.Area;
using Data.Character.Type;
using Data.Effect;
using UnityEngine;

namespace Data.Character
{
    public record CharacterMemento(
        string Name,
        ICharacterType CharacterType,
        CharacterStatusMemento Status,
        EntityMemento EntityData,
        InventoryMemento Inventory,
        AffiliationMemento Affiliation,
        bool IsLeader
    );
    public record EntityMemento(
        Vector2Int Position
    );
    public record CharacterStatusMemento(
        int MaxHp,
        int Hp,
        int Strength
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
        SkillMemento Skill,
        string Info
    );
    public record SkillMemento(
        IArea Area,
        IEffect Effect
    );
    public record AffiliationMemento(
        CharacterGroup Group
    );
}