#nullable enable
using Domain.Model.Character.Type;

namespace Domain.Model.Character
{
    public record CharacterMemento(
        string Name,
        ICharacterType CharacterType,
        bool wanderAround,
        CharacterStatusMemento Status,
        EntityMemento EntityData,
        SkillMemento[] Skills,
        InventoryMemento Inventory,
        AffiliationMemento Affiliation,
        Aggression Aggression,
        bool IsLeader,
        bool IsBoss
    );
}