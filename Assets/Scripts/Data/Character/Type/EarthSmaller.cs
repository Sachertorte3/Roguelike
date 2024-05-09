using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record EarthSmaller : ICharacterType
    {
        public EarthSmallerType Type; public string Name() => "EarthSmaller";
        public string TypeName() => $"{Name()}{Type}"; public EarthSmaller(EarthSmallerType type) { Type = type; }
    }
}