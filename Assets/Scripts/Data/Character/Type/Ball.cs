using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Ball : ICharacterType
    {
        public BallType Type; public string TypeName() => "Ball";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Ball(BallType type) { Type = type; }
    }
}