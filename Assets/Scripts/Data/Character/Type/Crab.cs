using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Crab : ICharacterType
    {
        public CrabType Type; public string TypeName() => "Crab";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Crab(CrabType type) { Type = type; }
    }
}