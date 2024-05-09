using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Sword : ICharacterType
    {
        public SwordType Type; public string TypeName() => "Sword";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Sword(SwordType type) { Type = type; }
    }
}