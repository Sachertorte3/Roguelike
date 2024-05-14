using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Crab : ICharacterType
    {
        public CrabType Type;

        public Crab(CrabType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Crab";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}