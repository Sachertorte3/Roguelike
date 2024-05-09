using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Sparks : ICharacterType
    {
        public SparksType Type; public string TypeName() => "Sparks";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Sparks(SparksType type) { Type = type; }
    }
}