using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Earth : ICharacterType
    {
        public EarthType Type; public string Name() => "Earth";
        public string TypeName() => $"{Name()}{Type}"; public Earth(EarthType type) { Type = type; }
    }
}