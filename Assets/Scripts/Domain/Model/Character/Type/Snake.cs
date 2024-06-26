using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Snake : ICharacterType
    {
        public SnakeType Type;

        public Snake(SnakeType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Snake";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}