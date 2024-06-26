using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record EarthSmall : ICharacterType
    {
        public EarthSmallType Type;

        public EarthSmall(EarthSmallType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "EarthSmall";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}