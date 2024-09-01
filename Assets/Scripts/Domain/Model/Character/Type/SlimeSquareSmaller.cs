using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record SlimeSquareSmaller : ICharacterType
    {
        public SlimeSquareSmallerType Type;

        public SlimeSquareSmaller(SlimeSquareSmallerType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "SlimeSquareSmaller";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}