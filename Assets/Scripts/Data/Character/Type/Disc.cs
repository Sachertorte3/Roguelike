using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Disc : ICharacterType
    {
        public DiscType Type; public string TypeName() => "Disc";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Disc(DiscType type) { Type = type; }
    }
}