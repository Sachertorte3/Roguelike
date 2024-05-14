using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Worm : ICharacterType
    {
        public WormType Type;

        public Worm(WormType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Worm";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}