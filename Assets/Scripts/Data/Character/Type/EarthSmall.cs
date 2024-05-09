using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record EarthSmall : ICharacterType
    {
        public EarthSmallType Type; public string Name() => "EarthSmall";
        public string TypeName() => $"{Name()}{Type}"; public EarthSmall(EarthSmallType type) { Type = type; }
    }
}