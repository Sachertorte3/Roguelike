using System;

namespace Data.Character.Type
{
    [Serializable]
    public record Golem : ICharacterType
    {
        public GolemType Type;

        public Golem(GolemType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Golem";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}