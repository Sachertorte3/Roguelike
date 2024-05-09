using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record EarthSmall : ICharacterType
    {
        public EarthSmallType Type; public string TypeName() => "EarthSmall";
        public string SubtypeName() => $"{TypeName()}{Type}"; public EarthSmall(EarthSmallType type) { Type = type; }
    }
}