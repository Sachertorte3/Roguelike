using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Earth : ICharacterType
    {
        public EarthType Type; public string TypeName() => "Earth";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Earth(EarthType type) { Type = type; }
    }
}