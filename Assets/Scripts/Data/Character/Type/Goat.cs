using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Goat : ICharacterType
    {
        public GoatType Type; public string TypeName() => "Goat";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Goat(GoatType type) { Type = type; }
    }
}