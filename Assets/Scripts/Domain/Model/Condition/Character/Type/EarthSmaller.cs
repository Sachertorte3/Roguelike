using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record EarthSmaller : ICharacterType
    {
        public EarthSmallerType Type;

        public EarthSmaller(EarthSmallerType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "EarthSmaller";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}