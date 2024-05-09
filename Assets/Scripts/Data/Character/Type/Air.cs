using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Air : ICharacterType
    {
        public AirType Type; public string TypeName() => "Air";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Air(AirType type) { Type = type; }
    }
}