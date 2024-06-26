using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Ball : ICharacterType
    {
        public BallType Type;

        public Ball(BallType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Ball";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}