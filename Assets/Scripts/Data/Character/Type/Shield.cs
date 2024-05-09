using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Shield : ICharacterType
    {
        public ShieldType Type; public string Name() => "Shield";
        public string TypeName() => $"{Name()}{Type}"; public Shield(ShieldType type) { Type = type; }
    }
}