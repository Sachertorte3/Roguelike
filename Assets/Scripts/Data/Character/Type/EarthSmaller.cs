using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record EarthSmaller : ICharacterType
    {
        public EarthSmallerType Type; public string TypeName() => "EarthSmaller";
        public string SubtypeName() => $"{TypeName()}{Type}"; public EarthSmaller(EarthSmallerType type) { Type = type; }
    }
}