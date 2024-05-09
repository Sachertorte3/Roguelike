using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record WaterSmall : ICharacterType
    {
        public WaterSmallType Type; public string TypeName() => "WaterSmall";
        public string SubtypeName() => $"{TypeName()}{Type}"; public WaterSmall(WaterSmallType type) { Type = type; }
    }
}