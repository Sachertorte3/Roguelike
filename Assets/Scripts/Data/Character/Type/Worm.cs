using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Worm : ICharacterType
    {
        public WormType Type; public string Name() => "Worm";
        public string TypeName() => $"{Name()}{Type}"; public Worm(WormType type) { Type = type; }
    }
}