using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Shield : ICharacterType
    {
        public ShieldType Type; public string TypeName() => "Shield";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Shield(ShieldType type) { Type = type; }
    }
}