using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Skull : ICharacterType
    {
        public SkullType Type;

        public Skull(SkullType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Skull";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}