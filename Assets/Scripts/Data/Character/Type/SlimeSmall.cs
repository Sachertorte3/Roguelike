using System;

namespace Data.Character.Type
{
    [Serializable]
    public record SlimeSmall : ICharacterType
    {
        public SlimeSmallType Type;

        public SlimeSmall(SlimeSmallType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "SlimeSmall";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}