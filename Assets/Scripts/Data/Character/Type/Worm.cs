using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Worm : ICharacterType
    {
        public WormType Type; public string TypeName() => "Worm";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Worm(WormType type) { Type = type; }
    }
}