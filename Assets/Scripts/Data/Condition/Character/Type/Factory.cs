using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Factory : ICharacterType
    {
        public FactoryType Type;

        public Factory(FactoryType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Factory";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}