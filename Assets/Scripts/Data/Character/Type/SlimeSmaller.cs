using System;

namespace Data.Character.Type
{
    [Serializable]
    public record SlimeSmaller : ICharacterType
    {
        public SlimeSmallerType Type;

        public SlimeSmaller(SlimeSmallerType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "SlimeSmaller";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}