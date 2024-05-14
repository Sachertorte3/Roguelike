using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Beard : ICharacterType
    {
        public BeardType Type;

        public Beard(BeardType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Beard";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}