using System;

namespace Data.Character.Type
{
    [Serializable]
    public record SlimeSquare : ICharacterType
    {
        public SlimeSquareType Type;

        public SlimeSquare(SlimeSquareType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "SlimeSquare";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}