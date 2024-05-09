using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Frog : ICharacterType
    {
        public FrogType Type; public string TypeName() => "Frog";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Frog(FrogType type) { Type = type; }
    }
}