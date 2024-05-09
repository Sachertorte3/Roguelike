using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Snake : ICharacterType
    {
        public SnakeType Type; public string TypeName() => "Snake";
        public string SubtypeName() => $"{TypeName()}{Type}"; public Snake(SnakeType type) { Type = type; }
    }
}