using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Crab : ICharacterType
    {
        public CrabType Type; public string Name() => "Crab";
        public string TypeName() => $"{Name()}{Type}"; public Crab(CrabType type) { Type = type; }
    }
}