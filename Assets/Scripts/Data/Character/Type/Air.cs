using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Air : ICharacterType
    {
        public AirType Type;

        public Air(AirType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Air";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}