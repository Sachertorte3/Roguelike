using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Water : ICharacterType
    {
        public WaterType Type; public string Name() => "Water";
        public string TypeName() => $"{Name()}{Type}"; public Water(WaterType type) { Type = type; }
    }
}