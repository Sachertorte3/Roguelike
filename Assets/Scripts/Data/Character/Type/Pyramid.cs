using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Pyramid : ICharacterType
    {
        public PyramidType Type; public string TypeName() => "Pyramid";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Pyramid(PyramidType type) { Type = type; }
    }
}