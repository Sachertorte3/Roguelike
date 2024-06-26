using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Shield : ICharacterType
    {
        public ShieldType Type;

        public Shield(ShieldType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Shield";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}