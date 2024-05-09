using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Ball : ICharacterType
    {
        public BallType Type; public string Name() => "Ball";
        public string TypeName() => $"{Name()}{Type}"; public Ball(BallType type) { Type = type; }
    }
}