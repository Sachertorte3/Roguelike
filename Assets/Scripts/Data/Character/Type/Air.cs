using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Air : ICharacterType
    {
        public AirType Type; public string Name() => "Air";
        public string TypeName() => $"{Name()}{Type}"; public Air(AirType type) { Type = type; }
    }
}