using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Pyramid : ICharacterType
    {
        public PyramidType Type; public string Name() => "Pyramid";
        public string TypeName() => $"{Name()}{Type}"; public Pyramid(PyramidType type) { Type = type; }
    }
}