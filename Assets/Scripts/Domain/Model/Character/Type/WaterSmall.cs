using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record WaterSmall : ICharacterType
    {
        public WaterSmallType Type;

        public WaterSmall(WaterSmallType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "WaterSmall";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}