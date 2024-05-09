using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Frog : ICharacterType
    {
        public FrogType Type; public string Name() => "Frog";
        public string TypeName() => $"{Name()}{Type}"; public Frog(FrogType type) { Type = type; }
    }
}