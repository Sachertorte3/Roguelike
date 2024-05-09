using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Skull : ICharacterType
    {
        public SkullType Type; public string Name() => "Skull";
        public string TypeName() => $"{Name()}{Type}"; public Skull(SkullType type) { Type = type; }
    }
}