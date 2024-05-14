using System;

namespace Data.Character.Type
{
    [Serializable]
    public record SlimeSquareSmall : ICharacterType
    {
        public SlimeSquareSmallType Type;

        public SlimeSquareSmall(SlimeSquareSmallType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "SlimeSquareSmall";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}