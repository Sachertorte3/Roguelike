using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Chicken : ICharacterType
    {
        public ChickenType Type;

        public Chicken(ChickenType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Chicken";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}