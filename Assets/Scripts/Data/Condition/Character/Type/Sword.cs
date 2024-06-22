using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Sword : ICharacterType
    {
        public SwordType Type;

        public Sword(SwordType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Sword";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}