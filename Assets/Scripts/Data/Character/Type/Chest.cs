using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Chest : ICharacterType
    {
        public ChestType Type; public string TypeName() => "Chest";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Chest(ChestType type) { Type = type; }
    }
}