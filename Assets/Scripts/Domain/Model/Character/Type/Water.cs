using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Water : ICharacterType
    {
        public WaterType Type;

        public Water(WaterType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Water";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}