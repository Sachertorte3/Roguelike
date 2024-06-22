using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Bat : ICharacterType
    {
        public BatType Type;

        public Bat(BatType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Bat";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}