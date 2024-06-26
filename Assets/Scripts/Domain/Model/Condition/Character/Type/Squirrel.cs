using System;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Squirrel : ICharacterType
    {
        public SquirrelType Type;

        public Squirrel(SquirrelType type)
        {
            Type = type;
        }

        public string TypeName()
        {
            return "Squirrel";
        }

        public string SubtypeName()
        {
            return $"{TypeName()}{Type}";
        }
    }
}