using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Water : ICharacterType
    {
        public WaterType Type; public string TypeName() => "Water";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Water(WaterType type) { Type = type; }
    }
}