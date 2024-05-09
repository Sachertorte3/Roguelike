using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Sparks : ICharacterType
    {
        public SparksType Type; public string Name() => "Sparks";
        public string TypeName() => $"{Name()}{Type}"; public Sparks(SparksType type) { Type = type; }
    }
}