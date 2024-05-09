using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Skull : ICharacterType
    {
        public SkullType Type; public string TypeName() => "Skull";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Skull(SkullType type) { Type = type; }
    }
}