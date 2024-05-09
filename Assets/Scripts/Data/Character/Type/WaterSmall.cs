using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record WaterSmall : ICharacterType
    {
        public WaterSmallType Type; public string Name() => "WaterSmall";
        public string TypeName() => $"{Name()}{Type}"; public WaterSmall(WaterSmallType type) { Type = type; }
    }
}