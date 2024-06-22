using System;

namespace Data.Character.Type
{
    [Serializable]
    public record SkullSmall : ICharacterType
    {
        public SkullSmallType Type;

        public SkullSmall(SkullSmallType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "SkullSmall";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}