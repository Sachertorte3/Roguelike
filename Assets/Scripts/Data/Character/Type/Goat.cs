using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Goat : ICharacterType
    {
        public GoatType Type; public string Name() => "Goat";
        public string TypeName() => $"{Name()}{Type}"; public Goat(GoatType type) { Type = type; }
    }
}