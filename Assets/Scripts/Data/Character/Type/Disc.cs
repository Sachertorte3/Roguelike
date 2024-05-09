using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Disc : ICharacterType
    {
        public DiscType Type; public string Name() => "Disc";
        public string TypeName() => $"{Name()}{Type}"; public Disc(DiscType type) { Type = type; }
    }
}