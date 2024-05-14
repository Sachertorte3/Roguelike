using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Monolith : ICharacterType
    {
        public MonolithType Type;

        public Monolith(MonolithType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Monolith";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}