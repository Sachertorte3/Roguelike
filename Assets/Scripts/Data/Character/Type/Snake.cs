using System;

namespace Database.Characters.Type
{
    [Serializable]
    public record Snake : ICharacterType
    {
        public SnakeType Type; public string Name() => "Snake";
        public string TypeName() => $"{Name()}{Type}"; public Snake(SnakeType type) { Type = type; }
    }
}