using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Sword : ICharacterType
    {
        public SwordType Type; public string Name() => "Sword";
        public string TypeName() => $"{Name()}{Type}"; public Sword(SwordType type) { Type = type; }
    }
}