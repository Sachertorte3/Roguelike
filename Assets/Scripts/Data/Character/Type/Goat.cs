using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Goat : ICharacterType
    {
        public GoatType Type;

        public Goat(GoatType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Goat";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}