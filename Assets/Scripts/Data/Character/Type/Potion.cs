using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Potion : ICharacterType
    {
        public PotionType Type;

        public Potion(PotionType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Potion";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}